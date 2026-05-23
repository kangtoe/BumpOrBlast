// 티어별 인벤토리 스택 상한 — 단일 출처.
// ItemInventory(런타임 캡)와 ItemData.MaxStack(개별 아이템 캡 = 같은 값) 모두 여기서 읽는다.
// 값 수정은 switch 분기에서 직접.
public static class kangtoe99_TierStackLimits
{
    public static int GetLimit(kangtoe99_Tier tier)
    {
        switch (tier)
        {
            case kangtoe99_Tier.Gray:   return 12;
            case kangtoe99_Tier.Green:  return 8;
            case kangtoe99_Tier.Blue:   return 5;
            case kangtoe99_Tier.Purple: return 3;
            case kangtoe99_Tier.Orange: return 1;
            default: return 0;
        }
    }
}
