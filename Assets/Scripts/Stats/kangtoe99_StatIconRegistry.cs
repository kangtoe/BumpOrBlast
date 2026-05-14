using UnityEngine;

// Stat별 아이콘 매핑. 단일 자산이 진리원천이 되어 모든 UI에서 동일 아이콘 사용.
// 권장 자산 경로: Assets/Data/Stats/StatIconRegistry.asset
[CreateAssetMenu(menuName = "BumpOrBlast/Stat Icon Registry", fileName = "StatIconRegistry")]
public class kangtoe99_StatIconRegistry : ScriptableObject
{
    [SerializeField] private kangtoe99_StatSpriteMap icons = new kangtoe99_StatSpriteMap();

    public Sprite GetIcon(kangtoe99_StatType stat)
    {
        icons.EnsureSize();
        return icons[stat];
    }

    // TMP Sprite Asset 사용 시 sprite name 컨벤션. R8 UI 적용 시 이 이름 사용.
    public static string GetSpriteTagName(kangtoe99_StatType stat) => stat.ToString();
}
