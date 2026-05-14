using UnityEngine;

[System.Serializable]
public class kangtoe99_StatModifierData
{
    [SerializeField] private kangtoe99_StatType stat;
    [SerializeField] private kangtoe99_ModifierKind kind;
    [SerializeField] private float value;

    public kangtoe99_StatType Stat => stat;
    public kangtoe99_ModifierKind Kind => kind;
    public float Value => value;

    public kangtoe99_IStatModifier CreateModifier(object source)
    {
        return new kangtoe99_StatModifier(stat, kind, value, source);
    }
}
