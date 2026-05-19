// 등급 — 적·아이템 공용 5단계 체계. 색상은 kangtoe99_TierColorPalette가 관리한다.
public enum kangtoe99_Tier
{
    Gray,
    Green,
    Blue,
    Purple,
    Orange
}

// 인게임 UI 표기용 레어도 이름 (영어 통일).
public static class kangtoe99_TierNames
{
    private static readonly string[] DisplayNames =
    {
        "Common",
        "Uncommon",
        "Rare",
        "Epic",
        "Legendary",
    };

    public static string GetDisplayName(kangtoe99_Tier tier)
    {
        int i = (int)tier;
        return (i >= 0 && i < DisplayNames.Length) ? DisplayNames[i] : string.Empty;
    }
}
