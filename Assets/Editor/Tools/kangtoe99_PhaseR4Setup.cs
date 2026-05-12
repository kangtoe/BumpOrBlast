// one-shot: Phase R4 검증 완료 후 삭제
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // 1. EnergySystem 컴포넌트 부착
        var energy = player.GetComponent<kangtoe99_EnergySystem>();
        if (energy == null)
        {
            energy = Undo.AddComponent<kangtoe99_EnergySystem>(player.gameObject);
            Debug.Log($"[PhaseR4Setup] '{player.name}'에 kangtoe99_EnergySystem 추가");
            changes++;
        }
        else
        {
            Debug.Log($"[PhaseR4Setup] '{player.name}'에 kangtoe99_EnergySystem 이미 존재 (스킵)");
        }

        // 2. EnergySystem.stats 슬롯 자동 할당
        changes += SetObjectReference(energy, "stats", stats);

        // 3. PlayerShooting.energy 슬롯 자동 할당
        var shooting = player.GetComponent<kangtoe99_PlayerShooting>();
        if (shooting != null)
        {
            changes += SetObjectReference(shooting, "energy", energy);
        }

        // 4. EnergyBarUI가 씬에 있으면 energySystem 슬롯 자동 할당
        var bars = Object.FindObjectsByType<kangtoe99_EnergyBarUI>(FindObjectsSortMode.None);
        foreach (var bar in bars)
        {
            changes += SetObjectReference(bar, "energySystem", energy);
        }

        if (bars.Length == 0)
        {
            Debug.Log("[PhaseR4Setup] 씬에 kangtoe99_EnergyBarUI가 없습니다. UI는 수동 셋업 필요 — 기존 AmmoUI 위치에 새 게이지 추가 권장.");
        }

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
