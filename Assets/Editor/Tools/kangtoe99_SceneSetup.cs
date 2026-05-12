using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 영구 도구. 현재 씬에 PlayerStats·EnergySystem·EnergyBar UI·기본 SO 자산이 갖춰져 있는지 점검하고
// 빠진 부분을 idempotent하게 채운다. 메뉴 반복 실행 안전.
public static class kangtoe99_SceneSetup
{
    private const string MenuPath = "Tools/BumpOrBlast/Setup Scene";
    private const string DataFolder = "Assets/Data/Players";
    private const string DefaultAssetPath = "Assets/Data/Players/PlayerStatsData_Default.asset";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        var player = Object.FindFirstObjectByType<kangtoe99_Player>();
        if (player == null)
        {
            Debug.LogError("[SceneSetup] 씬에서 kangtoe99_Player를 찾지 못했습니다.");
            return;
        }

        int changes = 0;

        var statsAsset = EnsureDefaultStatsAsset(ref changes);
        var stats = EnsureComponent<kangtoe99_PlayerStats>(player.gameObject, ref changes);
        var energy = EnsureComponent<kangtoe99_EnergySystem>(player.gameObject, ref changes);
        var shooting = player.GetComponent<kangtoe99_PlayerShooting>();
        var levelUp = Object.FindFirstObjectByType<kangtoe99_LevelUpSystem>();

        changes += SetRef(stats, "baseStatProfile", statsAsset);
        changes += SetRef(energy, "stats", stats);
        changes += SetRef(shooting, "stats", stats);
        changes += SetRef(shooting, "energy", energy);
        changes += SetRef(levelUp, "playerStats", stats);

        changes += DisableLegacyAmmoUI();
        var bar = EnsureEnergyBarUI(ref changes);
        changes += SetRef(bar, "energySystem", energy);

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[SceneSetup] 완료 — {changes}건 변경. 씬 저장 필요.");
        }
        else
        {
            Debug.Log("[SceneSetup] 모든 셋업이 이미 적용된 상태.");
        }
    }

    private static T EnsureComponent<T>(GameObject target, ref int changes) where T : Component
    {
        var c = target.GetComponent<T>();
        if (c == null)
        {
            c = Undo.AddComponent<T>(target);
            Debug.Log($"[SceneSetup] {target.name}에 {typeof(T).Name} 추가");
            changes++;
        }
        return c;
    }

    private static kangtoe99_PlayerStatsData EnsureDefaultStatsAsset(ref int changes)
    {
        var existing = AssetDatabase.LoadAssetAtPath<kangtoe99_PlayerStatsData>(DefaultAssetPath);
        if (existing != null) return existing;

        EnsureFolder(DataFolder);
        var asset = ScriptableObject.CreateInstance<kangtoe99_PlayerStatsData>();
        asset.SetBaseStats(kangtoe99_PlayerStats.Defaults);
        AssetDatabase.CreateAsset(asset, DefaultAssetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[SceneSetup] PlayerStatsData 자산 생성: {DefaultAssetPath}");
        changes++;
        return asset;
    }

    private static int DisableLegacyAmmoUI()
    {
        var ammo = Object.FindFirstObjectByType<kangtoe99_AmmoUIManager>(FindObjectsInactive.Include);
        if (ammo == null || !ammo.gameObject.activeSelf) return 0;

        Undo.RecordObject(ammo.gameObject, "Disable AmmoUI");
        ammo.gameObject.SetActive(false);
        Debug.Log($"[SceneSetup] 구버전 AmmoUI '{ammo.gameObject.name}' 비활성화");
        return 1;
    }

    private static kangtoe99_EnergyBarUI EnsureEnergyBarUI(ref int changes)
    {
        var existing = Object.FindFirstObjectByType<kangtoe99_EnergyBarUI>(FindObjectsInactive.Include);
        if (existing != null) return existing;

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SceneSetup] 씬에 Canvas가 없어 EnergyBar 자동 생성 스킵.");
            return null;
        }

        var bar = new GameObject("EnergyBar", typeof(RectTransform));
        bar.transform.SetParent(canvas.transform, false);
        var rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 24f);
        rt.sizeDelta = new Vector2(360f, 32f);

        var bg = AddUIChild(bar.transform, "Background", typeof(Image));
        var bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.55f);
        bgImg.raycastTarget = false;

        var fill = AddUIChild(bar.transform, "Fill", typeof(Image));
        var fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 1f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        fillImg.raycastTarget = false;

        var tickContainer = AddUIChild(bar.transform, "TickContainer", typeof(RectTransform));

        var ui = Undo.AddComponent<kangtoe99_EnergyBarUI>(bar);
        SetRef(ui, "fillImage", fillImg);
        SetRef(ui, "tickContainer", tickContainer.GetComponent<RectTransform>());

        Undo.RegisterCreatedObjectUndo(bar, "Create EnergyBar UI");
        Debug.Log("[SceneSetup] EnergyBar UI 자동 생성 (Canvas 하위)");
        changes++;
        return ui;
    }

    private static GameObject AddUIChild(Transform parent, string name, params System.Type[] components)
    {
        var go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        var parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private static int SetRef(Object target, string propertyName, Object value)
    {
        if (target == null || value == null) return 0;

        var so = new SerializedObject(target);
        var prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning($"[SceneSetup] {target.GetType().Name}에 '{propertyName}' 필드 없음");
            return 0;
        }
        if (prop.objectReferenceValue == value) return 0;

        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
        Debug.Log($"[SceneSetup] {target.GetType().Name}.{propertyName} ← {value.name}");
        return 1;
    }
}
