using UnityEngine;

[System.Serializable]
public class kangtoe99_StatModifierData
{
    [SerializeField] private kangtoe99_StatType stat;
    [SerializeField] private float value;

    public kangtoe99_StatType Stat => stat;
    // kind는 스텟에서 도출 — 자산엔 저장 안 함. StatKindRegistry가 단일 출처.
    public kangtoe99_ModifierKind Kind => kangtoe99_StatKindRegistry.GetKind(stat);
    public float Value => value;

    public kangtoe99_IStatModifier CreateModifier(object source)
    {
        return new kangtoe99_StatModifier(stat, Kind, value, source);
    }
}
