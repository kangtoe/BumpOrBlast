using UnityEngine;

// 등급별 색상을 한 곳에서 관리하는 공용 팔레트 SO.
// 적 틴트(EnemyTierData)·아이템 배경 UI 등이 같은 팔레트를 공유한다.
[CreateAssetMenu(fileName = "TierColorPalette", menuName = "kangtoe99/Tier Color Palette")]
public class kangtoe99_TierColorPalette : ScriptableObject
{
    [Tooltip("Gray→Orange 순서로 채울 것 (kangtoe99_Tier 순서)")]
    [SerializeField] private Color[] colors = new Color[]
    {
        new Color(0.6f, 0.6f, 0.6f),
        new Color(0.3f, 0.8f, 0.3f),
        new Color(0.3f, 0.5f, 1f),
        new Color(0.6f, 0.3f, 0.9f),
        new Color(1f, 0.5529412f, 0.1f),
    };

    public Color Get(kangtoe99_Tier tier)
    {
        int i = (int)tier;
        return (colors != null && i >= 0 && i < colors.Length) ? colors[i] : Color.white;
    }
}
