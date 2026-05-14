using UnityEditor;
using UnityEngine;

// R6a/R8a 검증용 샘플 자산 생성기.
// 메뉴: Tools > BumpOrBlast > Create Sample LevelUp Choices
// 한 번 실행 후 R7 진입 시점에 본 파일 폐기 검토.
// ItemData: displayName/description은 ItemData가 modifier에서 자동 조립.
// InstantDropItemData: 드롭 prefab은 Assets/Prefabs/Drops/ 경로에서 자동 연결.
public static class kangtoe99_SampleItemDataCreator
{
    private const string ItemsFolder = "Assets/Data/Items";
    private const string InstantDropsFolder = "Assets/Data/InstantDrops";

    [MenuItem("Tools/BumpOrBlast/Create Sample LevelUp Choices")]
    public static void CreateSamples()
    {
        EnsureFolder(ItemsFolder);
        EnsureFolder(InstantDropsFolder);

        int created = 0;
        // Gray-tier ItemData 3종
        created += CreateItem("ItemData_DamageUp_Gray",
            kangtoe99_ItemTier.Gray, 5,
            (kangtoe99_StatType.Damage, kangtoe99_ModifierKind.Multiplicative, 0.20f));
        created += CreateItem("ItemData_FireRate_Gray",
            kangtoe99_ItemTier.Gray, 5,
            (kangtoe99_StatType.FireRate, kangtoe99_ModifierKind.Multiplicative, -0.10f));
        created += CreateItem("ItemData_MoveForce_Gray",
            kangtoe99_ItemTier.Gray, 5,
            (kangtoe99_StatType.MoveForce, kangtoe99_ModifierKind.Multiplicative, 0.15f));

        // InstantDrop 3종 (드롭 prefab 자동 연결)
        created += CreateInstantDrop("InstantDropItem_XP",   "Assets/Prefabs/Drops/Item_exp.prefab",  "XP Boost", "즉시 XP 획득");
        created += CreateInstantDrop("InstantDropItem_HP",   "Assets/Prefabs/Drops/Item_hp.prefab",   "HP Pack",  "즉시 HP 회복");
        created += CreateInstantDrop("InstantDropItem_Bomb", "Assets/Prefabs/Drops/Item_boom.prefab", "Bomb",     "주변 적 일소");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SampleItemDataCreator] 생성 {created}개 (기존 자산은 스킵)");
    }

    private static int CreateItem(
        string assetName,
        kangtoe99_ItemTier tier,
        int maxStack,
        params (kangtoe99_StatType stat, kangtoe99_ModifierKind kind, float value)[] modifiers)
    {
        string assetPath = $"{ItemsFolder}/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<kangtoe99_ItemData>(assetPath) != null)
        {
            Debug.Log($"[SampleItemDataCreator] 스킵 (이미 존재): {assetPath}");
            return 0;
        }

        var data = ScriptableObject.CreateInstance<kangtoe99_ItemData>();
        AssetDatabase.CreateAsset(data, assetPath);

        var so = new SerializedObject(data);
        so.FindProperty("tier").enumValueIndex = (int)tier;
        so.FindProperty("maxStack").intValue = maxStack;

        var modsProp = so.FindProperty("modifiers");
        modsProp.arraySize = modifiers.Length;
        for (int i = 0; i < modifiers.Length; i++)
        {
            var elem = modsProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("stat").enumValueIndex = (int)modifiers[i].stat;
            elem.FindPropertyRelative("kind").enumValueIndex = (int)modifiers[i].kind;
            elem.FindPropertyRelative("value").floatValue = modifiers[i].value;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        Debug.Log($"[SampleItemDataCreator] 생성: {assetPath} → {data.Description.Replace('\n', ',')}");
        return 1;
    }

    private static int CreateInstantDrop(string assetName, string dropPrefabPath, string displayName, string description)
    {
        string assetPath = $"{InstantDropsFolder}/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<kangtoe99_InstantDropItemData>(assetPath) != null)
        {
            Debug.Log($"[SampleItemDataCreator] 스킵 (이미 존재): {assetPath}");
            return 0;
        }

        var dropComponent = AssetDatabase.LoadAssetAtPath<kangtoe99_Drop>(dropPrefabPath);
        if (dropComponent == null)
        {
            Debug.LogWarning($"[SampleItemDataCreator] Drop prefab 미발견: {dropPrefabPath} — {assetName} 생성 스킵");
            return 0;
        }

        var data = ScriptableObject.CreateInstance<kangtoe99_InstantDropItemData>();
        AssetDatabase.CreateAsset(data, assetPath);

        var so = new SerializedObject(data);
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("description").stringValue = description;
        so.FindProperty("dropPrefab").objectReferenceValue = dropComponent;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        Debug.Log($"[SampleItemDataCreator] 생성: {assetPath} → {description} (prefab: {dropPrefabPath})");
        return 1;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;
        var parts = folderPath.Split('/');
        string accum = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{accum}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(accum, parts[i]);
            }
            accum = next;
        }
    }
}
