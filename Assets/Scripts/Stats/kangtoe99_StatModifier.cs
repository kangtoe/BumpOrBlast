public class kangtoe99_StatModifier : kangtoe99_IStatModifier
{
    public kangtoe99_StatType Stat { get; }
    public kangtoe99_ModifierKind Kind { get; }
    public float Value { get; }
    public object Source { get; }

    public kangtoe99_StatModifier(kangtoe99_StatType stat, kangtoe99_ModifierKind kind, float value, object source = null)
    {
        Stat = stat;
        Kind = kind;
        Value = value;
        Source = source;
    }
}
