// 스텟별 최종값(GetFinal) 범위. PlayerStats가 이 값으로 Clamp한다.
// 무한대는 float.PositiveInfinity/NegativeInfinity로 표현.
// 정책: 재생계열(HPRegen/EnergyRegen) 0 하한, Luck은 음수 허용.
public static class kangtoe99_StatBoundsRegistry
{
    private const float Inf = float.PositiveInfinity;
    private const float NegInf = float.NegativeInfinity;

    public static float GetMin(kangtoe99_StatType stat)
    {
        switch (stat)
        {
            // 발사체
            case kangtoe99_StatType.ProjectileCount:    return 1f;
            case kangtoe99_StatType.ProjectileSpeed:    return 1f;
            case kangtoe99_StatType.ProjectileScale:    return 0.1f;
            case kangtoe99_StatType.ProjectileSpread:   return 0f;
            case kangtoe99_StatType.Pierce:             return 0f;

            // 무기
            case kangtoe99_StatType.Damage:             return 0f;
            case kangtoe99_StatType.FireRate:           return 0.02f;
            case kangtoe99_StatType.EnergyCost:         return 0f;

            // 에너지
            case kangtoe99_StatType.EnergyMax:          return 1f;
            case kangtoe99_StatType.EnergyRegen:        return 0f;

            // 기체
            case kangtoe99_StatType.MaxHP:              return 1f;
            case kangtoe99_StatType.HPRegen:            return 0f;
            case kangtoe99_StatType.BodyScale:          return 0.2f;

            // 이동
            case kangtoe99_StatType.MoveForce:          return 5f;
            case kangtoe99_StatType.RotationSpeed:      return 30f;
            case kangtoe99_StatType.Friction:           return 0f;

            // 메타
            case kangtoe99_StatType.Luck:               return NegInf;
            case kangtoe99_StatType.Magnet:             return 0f;

            // 물리/충돌
            case kangtoe99_StatType.SpeedCapOvershoot:  return 1f;
            case kangtoe99_StatType.CollisionKnockback: return 0f;

            default: return NegInf;
        }
    }

    public static float GetMax(kangtoe99_StatType stat)
    {
        switch (stat)
        {
            case kangtoe99_StatType.ProjectileCount:    return 30f;
            case kangtoe99_StatType.ProjectileScale:    return 10f;
            case kangtoe99_StatType.ProjectileSpread:   return 180f;
            case kangtoe99_StatType.BodyScale:          return 5f;
            default: return Inf;
        }
    }
}
