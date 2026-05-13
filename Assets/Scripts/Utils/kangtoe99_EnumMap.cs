using System;
using UnityEngine;

// 사용자 제안 기반. Unity 직렬화를 위해 readonly 제거 + [SerializeField].
// 제너릭 타입은 Unity 인스펙터에 직접 노출되지 않으므로 비제너릭 서브클래스로 감싸 사용.
// 예: public sealed class kangtoe99_StatMap : kangtoe99_EnumMap<kangtoe99_StatType, float> { }
[Serializable]
public class kangtoe99_EnumMap<TEnum, TValue>
    where TEnum : unmanaged, Enum
{
    [SerializeField] protected TValue[] values;

    public kangtoe99_EnumMap()
    {
        var enums = (TEnum[])Enum.GetValues(typeof(TEnum));
        values = new TValue[enums.Length];
    }

    public kangtoe99_EnumMap(Func<TEnum, TValue> initializer) : this()
    {
        var enums = (TEnum[])Enum.GetValues(typeof(TEnum));
        for (int i = 0; i < enums.Length; i++)
        {
            values[Convert.ToInt32(enums[i])] = initializer(enums[i]);
        }
    }

    public ref TValue this[TEnum e] => ref values[Convert.ToInt32(e)];

    public int Count => values?.Length ?? 0;

    // enum 항목이 추가/제거됐을 때 직렬화된 배열 크기를 enum 길이에 맞춘다.
    // 신규 항목은 default(TValue), 잘려나가는 항목은 데이터 손실.
    public void EnsureSize()
    {
        var enums = (TEnum[])Enum.GetValues(typeof(TEnum));
        if (values != null && values.Length == enums.Length) return;

        var resized = new TValue[enums.Length];
        if (values != null)
        {
            int copyLen = Math.Min(values.Length, enums.Length);
            Array.Copy(values, resized, copyLen);
        }
        values = resized;
    }

    public void CopyFrom(kangtoe99_EnumMap<TEnum, TValue> source)
    {
        if (source == null) return;
        EnsureSize();
        var enums = (TEnum[])Enum.GetValues(typeof(TEnum));
        foreach (var e in enums)
        {
            this[e] = source[e];
        }
    }
}
