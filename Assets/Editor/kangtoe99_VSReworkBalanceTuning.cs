using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class kangtoe99_VSReworkBalanceTuning
{
    private const string MenuPath = "Tools/BumpOrBlast/Apply Balance Tuning";

    // 적 스폰 (씬 EnemySpawner에 직접 대입)
    private const float SpawnIntervalInitial = 1.5f;
    private const float SpawnIntervalMin = 0.3f;
    private const float SpawnIntervalDecreaseRate = 0.05f;

    // 적 스탯 상한 (프리팹 값이 이보다 크면 내림 — idempotent, 반복 실행 안전)
    private const float EnemyMoveForceCap = 20f;
    private const float EnemyMaxSpeedCap = 2f;
    private const float EnemyKnockbackCap = 0.8f;

    [MenuItem(MenuPath)]
    public static void ApplyTuning()
    {
        int changes = 0;

        var spawner = Object.FindFirstObjectByType<kangtoe99_EnemySpawner>();
        if (spawner == null)
        {
            Debug.LogError("[BalanceTuning] kangtoe99_EnemySpawner를 찾지 못했습니다. 중단.");
            return;
        }

        changes += SetFloat(spawner, "initialSpawnInterval", SpawnIntervalInitial);
        changes += SetFloat(spawner, "minSpawnInterval", SpawnIntervalMin);
        changes += SetFloat(spawner, "intervalDecreaseRate", SpawnIntervalDecreaseRate);

        var so = new SerializedObject(spawner);
        var prefabsProp = so.FindProperty("enemyPrefabs");
        if (prefabsProp != null && prefabsProp.isArray)
        {
            for (int i = 0; i < prefabsProp.arraySize; i++)
            {
                var elem = prefabsProp.GetArrayElementAtIndex(i);
                if (elem.objectReferenceValue is GameObject prefab)
                {
                    changes += ApplyEnemyPrefabCaps(prefab);
                }
            }
        }

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[BalanceTuning] 완료 — {changes}개 변경 적용. Ctrl+S 로 씬 저장하세요.");
        }
        else
        {
            Debug.Log("[BalanceTuning] 변경사항 없음 — 이미 모두 기준 이하입니다.");
        }
    }

    private static int ApplyEnemyPrefabCaps(GameObject prefab)
    {
        if (prefab == null) return 0;
        string path = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"[BalanceTuning] 프리팹 '{prefab.name}': asset path 없음");
            return 0;
        }

        var contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var enemy = contents.GetComponent<kangtoe99_Enemy>();
            if (enemy == null) return 0;

            int delta = 0;
            var so = new SerializedObject(enemy);
            delta += CapFloat(so, "moveForce", EnemyMoveForceCap, prefab.name);
            delta += CapFloat(so, "maxSpeed", EnemyMaxSpeedCap, prefab.name);
            delta += CapFloat(so, "collisionKnockbackForce", EnemyKnockbackCap, prefab.name);

            if (delta > 0)
            {
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            return delta;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    // 값이 상한을 초과하면 상한으로 내림. 이미 이하면 유지.
    private static int CapFloat(SerializedObject so, string propName, float cap, string context)
    {
        var prop = so.FindProperty(propName);
        if (prop == null) return 0;
        if (prop.floatValue <= cap) return 0;
        float old = prop.floatValue;
        prop.floatValue = cap;
        Debug.Log($"[BalanceTuning] '{context}'.{propName}: {old} → {cap}");
        return 1;
    }

    private static int SetFloat(Object target, string propName, float value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(propName);
        if (prop == null)
        {
            Debug.LogWarning($"[BalanceTuning] {target.GetType().Name}에 '{propName}' 프로퍼티 없음");
            return 0;
        }
        if (Mathf.Approximately(prop.floatValue, value)) return 0;

        Undo.RecordObject(target, $"Set {propName}");
        float old = prop.floatValue;
        prop.floatValue = value;
        so.ApplyModifiedProperties();
        Debug.Log($"[BalanceTuning] {target.GetType().Name}.{propName}: {old} → {value}");
        return 1;
    }
}
