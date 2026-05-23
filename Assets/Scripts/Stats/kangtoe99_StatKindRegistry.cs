// 스텟별 적용 방식(Additive=고정수치 / Multiplicative=퍼센트)의 단일 출처.
// 모디파이어는 stat과 value만 보유, kind는 여기서 도출 — 한 스텟이 두 방식을 동시에 가지지 않는다.
// 값 수정은 switch 분기에서 직접.
public static class kangtoe99_StatKindRegistry
{
    public static kangtoe99_ModifierKind GetKind(kangtoe99_StatType stat)
    {
        switch (stat)
        {
            // Additive — 정수·각도·메타 스코어
            case kangtoe99_StatType.ProjectileCount:    return kangtoe99_ModifierKind.Additive;
            case kangtoe99_StatType.ProjectileSpread:   return kangtoe99_ModifierKind.Additive;
            case kangtoe99_StatType.Pierce:             return kangtoe99_ModifierKind.Additive;
            case kangtoe99_StatType.Luck:               return kangtoe99_ModifierKind.Additive;
            case kangtoe99_StatType.RotationSpeed:      return kangtoe99_ModifierKind.Additive;  // 사실상 미사용

            // Multiplicative — 연속 스칼라·비율·물리
            case kangtoe99_StatType.ProjectileSpeed:    return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.ProjectileScale:    return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.Damage:             return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.FireRate:           return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.EnergyCost:         return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.EnergyMax:          return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.EnergyRegen:        return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.MaxHP:              return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.HPRegen:            return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.BodyScale:          return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.MoveForce:          return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.Friction:           return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.Magnet:             return kangtoe99_ModifierKind.Multiplicative;
            case kangtoe99_StatType.SpeedCapOvershoot:  return kangtoe99_ModifierKind.Multiplicative;  // 사실상 미사용
            case kangtoe99_StatType.CollisionKnockback: return kangtoe99_ModifierKind.Multiplicative;

            default: return kangtoe99_ModifierKind.Additive;
        }
    }
}
