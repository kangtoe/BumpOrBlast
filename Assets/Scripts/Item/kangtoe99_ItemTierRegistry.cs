using System.Collections.Generic;

// ItemData -> Tier 매핑 단일 출처.
// LevelUpSystem이 Resources에서 티어 폴더별 로드 시 Register() 호출 → 다른 코드는 GetTier()로 조회.
// ItemData에는 더 이상 serialized tier 필드가 없으며 ItemData.Tier 프로퍼티가 여기를 거친다.
public static class kangtoe99_ItemTierRegistry
{
    private static readonly Dictionary<kangtoe99_ItemData, kangtoe99_Tier> map = new Dictionary<kangtoe99_ItemData, kangtoe99_Tier>();

    public static void Clear() => map.Clear();

    public static void Register(kangtoe99_ItemData item, kangtoe99_Tier tier)
    {
        if (item == null) return;
        map[item] = tier;
    }

    // 미등록 자산은 Gray로 폴백. 보통 LevelUpSystem.Awake 전에는 채워지지 않음.
    public static kangtoe99_Tier GetTier(kangtoe99_ItemData item)
    {
        return (item != null && map.TryGetValue(item, out var t)) ? t : kangtoe99_Tier.Gray;
    }

    public static bool HasTier(kangtoe99_ItemData item)
    {
        return item != null && map.ContainsKey(item);
    }
}
