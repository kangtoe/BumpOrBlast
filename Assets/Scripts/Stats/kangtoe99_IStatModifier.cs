public enum kangtoe99_ModifierKind
{
    Additive,
    Multiplicative
}

public interface kangtoe99_IStatModifier
{
    kangtoe99_StatType Stat { get; }
    kangtoe99_ModifierKind Kind { get; }
    float Value { get; }
    object Source { get; }
}
