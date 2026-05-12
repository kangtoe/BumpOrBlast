// one-shot: Phase R4 검증 완료 후 삭제
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class kangtoe99_PhaseR4Setup
{
    private const string MenuPath = "Tools/BumpOrBlast/Setup Scene (Phase R4 - Energy)";

    [MenuItem(MenuPath)]
    public static void SetupScene()
    {
        int changes = 0;

        var player = Object.FindFirstObjectByType<kangtoe99_Player>();
        if (player == null)
        {
            Debug.LogError("[PhaseR4Setup] 씬에서 kangtoe99_Player를 찾지 못했습니다.");
            return;
        }

        var stats = player.GetComponent<kangtoe99_PlayerStats>();
        if (stats == null)
        {
            Debug.LogError("[PhaseR4Setup] kangtoe99_PlayerStats가 없습니다. 먼저 Phase R3 Setup을 실행하세요.");
            return;
        }

        // 1. EnergySystem 부착
        var energy = player.GetComponent<kangtoe99_EnergySystem>();
        if (energy == null)
        {
            energy = Undo.AddComponent<kangtoe99_EnergySystem>(player.gameObject);
            Debug.Log($"[PhaseR4Setup] '{player.name}'에 kangtoe99_EnergySystem 추가");
            changes++;
        }

        changes += SetObjectReference(energy, "stats", stats);

        // 2. PlayerShooting.energy 자동 할당
        var shooting = player.GetComponent<kangtoe99_PlayerShooting>();
        if (shooting != null)
        {
            changes += SetObjectReference(shooting, "energy", energy);
        }

        // 3. 기존 AmmoUI 비활성화
        var ammoUI = Object.FindFirstObjectByType<kangtoe99_AmmoUIManager>(FindObjectsInactive.Include);
        if (ammoUI != null && ammoUI.gameObject.activeSelf)
        {
            Undo.RecordObject(ammoUI.gameObject, "Disable AmmoUI");
            ammoUI.gameObject.SetActive(false);
            Debug.Log($"[PhaseR4Setup] 기존 AmmoUIManager '{ammoUI.gameObject.name}' 비활성화");
            changes++;
        }

        // 4. EnergyBar UI 자동 생성 또는 기존 EnergyBarUI에 슬롯 할당
        var bar = Object.FindFirstObjectByType<kangtoe99_EnergyBarUI>(FindObjectsInactive.Include);
        if (bar == null)
        {
            bar = CreateEnergyBarUI(out int barChanges);
            changes += barChanges;
        }
        if (bar != null)
        {
            changes += SetObjectReference(bar, "energySystem", energy);
        }

        // 5. 카메라 추적/그리드 배경 잔재 제거 (코드 삭제로 missing 컴포넌트 가능성)
        changes += CleanupObsoleteComponents();

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[PhaseR4Setup] 완료 — 총 {changes}건 변경. 씬 저장 필요.");
        }
        else
        {
            Debug.Log("[PhaseR4Setup] 모든 셋업이 이미 적용된 상태.");
        }
    }

    private static kangtoe99_EnergyBarUI CreateEnergyBarUI(out int changes)
    {
        changes = 0;
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[PhaseR4Setup] 씬에 Canvas가 없습니다. EnergyBar 자동 생성 스킵.");
            return null;
        }

        // 부모: 빈 RectTransform
        var bar = new GameObject("EnergyBar", typeof(RectTransform));
        bar.transform.SetParent(canvas.transform, false);
        var barRt = bar.GetComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0.5f, 0f);
        barRt.anchorMax = new Vector2(0.5f, 0f);
        barRt.pivot = new Vector2(0.5f, 0f);
        barRt.anchoredPosition = new Vector2(0f, 24f);
        barRt.sizeDelta = new Vector2(360f, 32f);

        // 배경
        var bg = CreateUIChild(bar.transform, "Background", typeof(Image));
        var bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.55f);
        StretchToParent(bg.GetComponent<RectTransform>());

        // 채움
        var fill = CreateUIChild(bar.transform, "Fill", typeof(Image));
        var fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 1f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        StretchToParent(fill.GetComponent<RectTransform>());

        // 텍스트
        var textObj = CreateUIChild(bar.transform, "ValueText", typeof(Text));
        var text = textObj.GetComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = "10/10";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        StretchToParent(textObj.GetComponent<RectTransform>());

        // 컴포넌트 부착 + 슬롯 할당
        var ui = Undo.AddComponent<kangtoe99_EnergyBarUI>(bar);
        SetObjectReference(ui, "fillImage", fillImg);
        SetObjectReference(ui, "valueText", text);

        Undo.RegisterCreatedObjectUndo(bar, "Create EnergyBar UI");
        Debug.Log("[PhaseR4Setup] EnergyBar UI 자동 생성 (Canvas 하위)");
        changes++;
        return ui;
    }

    private static GameObject CreateUIChild(Transform parent, string name, params System.Type[] components)
    {
        var go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        if (go.GetComponent<RectTransform>() == null)
        {
            go.AddComponent<RectTransform>();
        }
        return go;
    }

    private static void StretchToParent(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // 카메라 추적·그리드 배경 잔재 정리 (코드 삭제로 missing 컴포넌트 가능)
    private static int CleanupObsoleteComponents()
    {
        int changes = 0;

        var camera = Camera.main;
        if (camera != null)
        {
            var components = camera.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null) // missing script
                {
                    Debug.LogWarning($"[PhaseR4Setup] Camera에 missing 컴포넌트가 있습니다. 인스펙터에서 'Remove Missing'을 실행해 주세요.");
                    break;
                }
            }
        }

        // GridBackground 게임오브젝트 비활성화 (이름으로 탐색)
        var gridBg = GameObject.Find("GridBackground");
        if (gridBg != null && gridBg.activeSelf)
        {
            Undo.RecordObject(gridBg, "Disable GridBackground");
            gridBg.SetActive(false);
            Debug.Log("[PhaseR4Setup] GridBackground 게임오브젝트 비활성화");
            changes++;
        }

        return changes;
    }

    private static int SetObjectReference(Object target, string propertyName, Object value)
    {
        if (target == null || value == null) return 0;

        var so = new SerializedObject(target);
        var prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning($"[PhaseR4Setup] {target.GetType().Name}에 '{propertyName}' 필드 없음");
            return 0;
        }

        if (prop.objectReferenceValue == value) return 0;

        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
        Debug.Log($"[PhaseR4Setup] {target.GetType().Name}.{propertyName} ← {value.name}");
        return 1;
    }
}
