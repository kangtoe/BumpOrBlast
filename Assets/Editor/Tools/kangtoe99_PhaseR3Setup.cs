// one-shot: Phase R3 검증 완료 후 삭제
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class kangtoe99_PhaseR3Setup
{
    private const string MenuPath = "Tools/BumpOrBlast/Setup Scene (Phase R3 - PlayerStats)";
    private const string DataFolder = "Assets/Data/Players";
    private const string DefaultAssetPath = "Assets/Data/Players/PlayerStatsData_Default.asset";

    [MenuItem(MenuPath)]
    public static void SetupScene()
    {
        int changes = 0;

        var player = Object.FindFirstObjectByType<kangtoe99_Player>();
        if (player == null)
        {
            Debug.LogError("[PhaseR3Setup] 씬에서 kangtoe99_Player를 찾지 못했습니다.");
            return;
        }

        // 1. PlayerStats 컴포넌트 부착
        var stats = player.GetComponent<kangtoe99_PlayerStats>();
        if (stats == null)
        {
            stats = Undo.AddComponent<kangtoe99_PlayerStats>(player.gameObject);
            Debug.Log($"[PhaseR3Setup] '{player.name}'에 kangtoe99_PlayerStats 추가");
            changes++;
        }

        // 2. PlayerStatsData_Default.asset 자동 생성 또는 로드
        var statsData = EnsureDefaultStatsAsset();

        // 3. PlayerStats.baseStatProfile 슬롯 자동 할당
        if (statsData != null)
        {
            changes += SetObjectReference(stats, "baseStatProfile", statsData);
        }

        // 4. PlayerShooting.stats 슬롯 자동 할당
        var shooting = player.GetComponent<kangtoe99_PlayerShooting>();
        if (shooting != null)
        {
            changes += SetObjectReference(shooting, "stats", stats);
        }

        // 5. LevelUpSystem.playerStats 슬롯 자동 할당
        var lus = Object.FindFirstObjectByType<kangtoe99_LevelUpSystem>();
        if (lus != null)
        {
            changes += SetObjectReference(lus, "playerStats", stats);
        }

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[PhaseR3Setup] 완료 — 총 {changes}건 변경. 씬 저장 필요.");
        }
        else
        {
            Debug.Log("[PhaseR3Setup] 모든 셋업이 이미 적용된 상태.");
        }
    }

    private static kangtoe99_PlayerStatsData EnsureDefaultStatsAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<kangtoe99_PlayerStatsData>(DefaultAssetPath);
        if (existing != null)
        {
            Debug.Log($"[PhaseR3Setup] 기존 PlayerStatsData 자산 사용: {DefaultAssetPath}");
            return existing;
        }

        EnsureFolder(DataFolder);

        var asset = ScriptableObject.CreateInstance<kangtoe99_PlayerStatsData>();
        asset.SetBaseStats(kangtoe99_PlayerStats.Defaults);

        AssetDatabase.CreateAsset(asset, DefaultAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PhaseR3Setup] PlayerStatsData 자산 생성: {DefaultAssetPath} (코드 Defaults로 초기화)");
        return asset;
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
                Debug.Log($"[PhaseR3Setup] 폴더 생성: {next}");
            }
            current = next;
        }
    }

    private static int SetObjectReference(Object target, string propertyName, Object value)
    {
        if (target == null || value == null) return 0;

        var so = new SerializedObject(target);
        var prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning($"[PhaseR3Setup] {target.GetType().Name}에 '{propertyName}' 필드 없음");
            return 0;
        }

        if (prop.objectReferenceValue == value) return 0;

        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
        Debug.Log($"[PhaseR3Setup] {target.GetType().Name}.{propertyName} ← {value.name}");
        return 1;
    }
}
