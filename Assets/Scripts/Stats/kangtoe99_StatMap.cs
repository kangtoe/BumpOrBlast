using System;

// Unity 인스펙터에 노출되기 위해 비제너릭 서브클래스로 명시.
[Serializable]
public sealed class kangtoe99_StatMap : kangtoe99_EnumMap<kangtoe99_StatType, float>
{
    public kangtoe99_StatMap() : base() { }
    public kangtoe99_StatMap(Func<kangtoe99_StatType, float> initializer) : base(initializer) { }
}
