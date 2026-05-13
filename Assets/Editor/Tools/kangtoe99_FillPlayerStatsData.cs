// one-shot: PlayerStatsData_Default.asset에 합리적 기본값을 주입. 실행 후 이 파일 삭제 권장.
using UnityEditor;
using UnityEngine;

public static class kangtoe99_FillPlayerStatsData
{
    private const string MenuPath = "Tools/BumpOrBlast/Fill PlayerStatsData_Default";
    private const string DataFolder = "Assets/Data/Players";
    private const string AssetPath = "Assets/Data/Players/PlayerStatsData_Default.asset";

    [MenuItem(MenuPath)]
    public static void Fill()
    {
        var asset = AssetDatabase.LoadAssetAtPath<kangtoe99_PlayerStatsData>(AssetPath);
        if (asset == null)
        {
            EnsureFolder(DataFolder);
            asset = ScriptableObject.CreateInstance<kangtoe99_PlayerStatsData>();
            AssetDatabase.CreateAsset(asset, AssetPath);
            Debug.Log($"[FillPlayerStatsData] 자산 생성: {AssetPath}");
        }

        var map = asset.BaseStats;

        // 발사체
        map[kangtoe99_StatType.ProjectileCount] = 1f;
        map[kangtoe99_StatType.ProjectileSpeed] = 18f;
        map[kangtoe99_StatType.ProjectileScale] = 1f;
        map[kangtoe99_StatType.ProjectileSpread] = 0f;
        map[kangtoe99_StatType.Pierce] = 0f;

        // 무기
        map[kangtoe99_StatType.Damage] = 10f;
        map[kangtoe99_StatType.FireRate] = 0.3f;
        map[kangtoe99_StatType.EnergyCost] = 1f;

        // 에너지
        map[kangtoe99_StatType.EnergyMax] = 12f;
        map[kangtoe99_StatType.EnergyRegen] = 4f;

        // 기체
        map[kangtoe99_StatType.MaxHP] = 100f;
        map[kangtoe99_StatType.HPRegen] = 0.5f;
        map[kangtoe99_StatType.BodyScale] = 1f;

        // 이동 — 평형 속도 ≈ MoveForce / Friction = 50/8 ≈ 6.25
        map[kangtoe99_StatType.MoveForce] = 50f;
        map[kangtoe99_StatType.RotationSpeed] = 270f;
        map[kangtoe99_StatType.Friction] = 8f;

        // 메타
        map[kangtoe99_StatType.Luck] = 0f;
        map[kangtoe99_StatType.Magnet] = 2f;

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[FillPlayerStatsData] PlayerStatsData_Default 값 주입 완료. 도구 파일은 검증 후 삭제.");
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
}
