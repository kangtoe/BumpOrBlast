// one-shot: Phase R3 검증 완료 후 삭제
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class kangtoe99_PhaseR3Setup
{
    private const string MenuPath = "Tools/BumpOrBlast/Setup Scene (Phase R3 - PlayerStats)";

    [MenuItem(MenuPath)]
    public static void SetupScene()
    {
        int changes = 0;

        var player = Object.FindFirstObjectByType<kangtoe99_Player>();
        if (player == null)
        {
            Debug.LogError("[PhaseR3Setup] 씬에서 kangtoe99_Player를 찾지 못했습니다. SampleScene 등 플레이어가 있는 씬을 연 상태로 실행하세요.");
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
        else
        {
            Debug.Log($"[PhaseR3Setup] '{player.name}'에 kangtoe99_PlayerStats 이미 존재 (스킵)");
        }

        // 2. PlayerShooting.stats 슬롯 자동 할당
        var shooting = player.GetComponent<kangtoe99_PlayerShooting>();
        if (shooting != null)
        {
            changes += SetObjectReference(shooting, "stats", stats);
        }
        else
        {
            Debug.LogWarning("[PhaseR3Setup] PlayerCharacter에 kangtoe99_PlayerShooting이 없음 — stats 슬롯 자동 할당 스킵");
        }

        // 3. LevelUpSystem.playerStats 슬롯 자동 할당
        var lus = Object.FindFirstObjectByType<kangtoe99_LevelUpSystem>();
        if (lus != null)
        {
            changes += SetObjectReference(lus, "playerStats", stats);
        }
        else
        {
            Debug.LogWarning("[PhaseR3Setup] 씬에 kangtoe99_LevelUpSystem이 없음 — slot 자동 할당 스킵");
        }

        // 4. Character의 SerializeField 기본값을 PlayerStats Defaults와 동기화하지 않음 — 사용자가 의도해서 변경했을 수도 있으므로 인스펙터 'Overrides'로 명시 추가 권장
        // 변경 사항 마크
        if (changes > 0)
        {
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[PhaseR3Setup] 완료 — 총 {changes}건 변경. 씬 저장 필요.");
        }
        else
        {
            Debug.Log("[PhaseR3Setup] 모든 셋업이 이미 적용된 상태 (변경 없음).");
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

        if (prop.objectReferenceValue == value)
        {
            return 0;
        }

        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
        Debug.Log($"[PhaseR3Setup] {target.GetType().Name}.{propertyName} ← {value.name}");
        return 1;
    }
}
