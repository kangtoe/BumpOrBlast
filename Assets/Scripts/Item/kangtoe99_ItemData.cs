using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "BumpOrBlast/Item Data", fileName = "ItemData_")]
public class kangtoe99_ItemData : ScriptableObject, kangtoe99_ILevelUpChoice
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Modifiers (applied per stack)")]
    [SerializeField] private List<kangtoe99_StatModifierData> modifiers = new List<kangtoe99_StatModifierData>();

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public Sprite Icon => icon;
    // 티어는 Resources/Items/Tier{N}_* 폴더 위치가 진리원천 — 자산 자체엔 저장 안 함.
    // LevelUpSystem.LoadItemPool이 Awake에서 ItemTierRegistry에 등록.
    public kangtoe99_Tier Tier => kangtoe99_ItemTierRegistry.GetTier(this);
    public int MaxStack => kangtoe99_TierStackLimits.GetLimit(Tier);
    public IReadOnlyList<kangtoe99_StatModifierData> Modifiers => modifiers;

    // 디버그·툴팁용 풀텍스트 (stat 이름 + 값). UI에서 아이콘을 쓰면 FormatValue만 호출.
    // R6b에서 트리거 효과가 추가되면 같은 자리에서 합쳐 표시.
    public string Description
    {
        get
        {
            if (modifiers == null || modifiers.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append(FormatModifier(modifiers[i]));
            }
            return sb.ToString();
        }
    }

    public bool IsAvailable(GameObject player)
    {
        if (player == null) return false;
        var inv = player.GetComponent<kangtoe99_ItemInventory>();
        return inv != null && !inv.IsFull(this);
    }

    public void Apply(GameObject player)
    {
        if (player == null) return;
        var inv = player.GetComponent<kangtoe99_ItemInventory>();
        inv?.TryAdd(this);
    }

    public static string FormatModifier(kangtoe99_StatModifierData m)
        => $"{m.Stat} {FormatValue(m)}";

    // 부호 포함 값만 ("+20%" / "-10%" / "+5"). UI에서 아이콘과 함께 쓸 때 사용.
    // Multiplicative value는 이미 퍼센트 단위(예: -20 = -20%)로 저장되므로 그대로 표시.
    public static string FormatValue(kangtoe99_StatModifierData m)
    {
        string sign = m.Value >= 0f ? "+" : string.Empty;
        if (m.Kind == kangtoe99_ModifierKind.Multiplicative)
        {
            return $"{sign}{m.Value:0.#}%";
        }
        return $"{sign}{m.Value:0.##}";
    }
}
