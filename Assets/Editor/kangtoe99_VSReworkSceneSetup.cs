using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class kangtoe99_VSReworkSceneSetup
{
    private const string MenuPath = "Tools/BumpOrBlast/Setup Scene (VS Rework - Phase R1)";

    [MenuItem(MenuPath)]
    public static void SetupScene()
    {
        int changes = 0;

        var player = Object.FindFirstObjectByType<kangtoe99_Player>();
        if (player == null)
        {
            Debug.LogError("[VSReworkSceneSetup] kangtoe99_Player를 씬에서 찾지 못했습니다. 중단.");
            return;
        }

        // 물리: Player Rigidbody2D interpolation
        changes += EnsureRigidbodyInterpolation(player);

        // 카메라: CameraFollow 컴포넌트 + 타겟 연결
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogError("[VSReworkSceneSetup] Main Camera를 찾지 못했습니다. 중단.");
            return;
        }

        changes += EnsureComponent<kangtoe99_CameraFollow>(cam.gameObject);
        changes += SetObjectReference(cam.GetComponent<kangtoe99_CameraFollow>(), "target", player.transform);

        // 물리: 적 프리팹 Rigidbody2D interpolation
        var spawner = Object.FindFirstObjectByType<kangtoe99_EnemySpawner>();
        if (spawner != null)
        {
            var spawnerSO = new SerializedObject(spawner);
            var prefabsProp = spawnerSO.FindProperty("enemyPrefabs");
            if (prefabsProp != null && prefabsProp.isArray)
            {
                for (int i = 0; i < prefabsProp.arraySize; i++)
                {
                    var elem = prefabsProp.GetArrayElementAtIndex(i);
                    if (elem.objectReferenceValue is GameObject prefab)
                    {
                        changes += SetPrefabRigidbodyInterpolation(prefab);
                    }
                }
            }
        }

        // 물리: 총알 프리팹 Rigidbody2D interpolation
        var playerShooting = Object.FindFirstObjectByType<kangtoe99_PlayerShooting>();
        if (playerShooting != null)
        {
            var psSO = new SerializedObject(playerShooting);
            var bulletProp = psSO.FindProperty("bulletPrefab");
            if (bulletProp != null && bulletProp.objectReferenceValue is GameObject bulletPrefab)
            {
                changes += SetPrefabRigidbodyInterpolation(bulletPrefab);
            }
        }

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[VSReworkSceneSetup] 완료 — {changes}개 변경 적용. Ctrl+S 로 씬 저장하세요.");
        }
        else
        {
            Debug.Log("[VSReworkSceneSetup] 변경사항 없음 — 이미 모두 설정되어 있습니다.");
        }
    }

    private static int EnsureComponent<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() != null) return 0;
        Undo.AddComponent<T>(go);
        Debug.Log($"[VSReworkSceneSetup] {go.name}에 {typeof(T).Name} 추가");
        return 1;
    }

    private static int EnsureRigidbodyInterpolation(Component owner)
    {
        var rb = owner.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning($"[VSReworkSceneSetup] {owner.name}: Rigidbody2D 없음 — interpolation 설정 스킵");
            return 0;
        }
        if (rb.interpolation == RigidbodyInterpolation2D.Interpolate)
        {
            Debug.Log($"[VSReworkSceneSetup] {owner.name}: Rigidbody2D.interpolation 이미 Interpolate (변경 없음)");
            return 0;
        }

        Undo.RecordObject(rb, "Set Rigidbody2D Interpolation");
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        EditorUtility.SetDirty(rb);
        Debug.Log($"[VSReworkSceneSetup] {owner.name}: Rigidbody2D.interpolation None → Interpolate");
        return 1;
    }

    private static int SetObjectReference(Object target, string propertyName, Object value)
    {
        if (target == null || value == null) return 0;

        var so = new SerializedObject(target);
        var prop = so.FindProperty(propertyName);
        if (prop == null) return 0;
        if (prop.objectReferenceValue == value) return 0;

        Undo.RecordObject(target, $"Set {propertyName}");
        prop.objectReferenceValue = value;
        so.ApplyModifiedProperties();
        Debug.Log($"[VSReworkSceneSetup] {target.GetType().Name}.{propertyName} → {value.name}");
        return 1;
    }

    private static int SetPrefabRigidbodyInterpolation(GameObject prefab)
    {
        if (prefab == null) return 0;
        string path = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"[VSReworkSceneSetup] 프리팹 '{prefab.name}': asset path 없음");
            return 0;
        }

        var contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var rb = contents.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning($"[VSReworkSceneSetup] 프리팹 '{prefab.name}': Rigidbody2D 없음");
                return 0;
            }
            if (rb.interpolation == RigidbodyInterpolation2D.Interpolate)
            {
                Debug.Log($"[VSReworkSceneSetup] 프리팹 '{prefab.name}': 이미 Interpolate (변경 없음)");
                return 0;
            }

            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            Debug.Log($"[VSReworkSceneSetup] 프리팹 '{prefab.name}': Rigidbody2D.interpolation None → Interpolate");
            return 1;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
