using System;
using System.Collections.Generic;
using UnityEngine;

public class kangtoe99_PlayerStats : MonoBehaviour
{
    [Header("Base Stat Profile (SO)")]
    [SerializeField] private kangtoe99_PlayerStatsData baseStatProfile;

    private kangtoe99_StatMap baseValues;
    private List<kangtoe99_IStatModifier> modifiers = new List<kangtoe99_IStatModifier>();

    public event Action<kangtoe99_StatType> OnStatChanged;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (baseValues != null) return;

        baseValues = new kangtoe99_StatMap();

        if (baseStatProfile != null)
        {
            baseValues.CopyFrom(baseStatProfile.BaseStats);
        }
        else
        {
            Debug.LogWarning("[kangtoe99_PlayerStats] baseStatProfile(SO)이 할당되지 않았습니다. 모든 스탯이 0으로 시작합니다.");
        }
    }

    public float GetBase(kangtoe99_StatType stat)
    {
        EnsureInitialized();
        return baseValues[stat];
    }

    public void SetBase(kangtoe99_StatType stat, float value)
    {
        EnsureInitialized();
        baseValues[stat] = value;
        OnStatChanged?.Invoke(stat);
    }

    public float GetFinal(kangtoe99_StatType stat)
    {
        float baseVal = GetBase(stat);
        float additive = 0f;
        float multiplicative = 1f;

        for (int i = 0; i < modifiers.Count; i++)
        {
            var m = modifiers[i];
            if (m.Stat != stat) continue;
            if (m.Kind == kangtoe99_ModifierKind.Additive) additive += m.Value;
            else multiplicative *= (1f + m.Value);
        }

        return (baseVal + additive) * multiplicative;
    }

    public void AddModifier(kangtoe99_IStatModifier modifier)
    {
        if (modifier == null) return;
        modifiers.Add(modifier);
        OnStatChanged?.Invoke(modifier.Stat);
    }

    public void RemoveModifier(kangtoe99_IStatModifier modifier)
    {
        if (modifier == null) return;
        if (modifiers.Remove(modifier))
        {
            OnStatChanged?.Invoke(modifier.Stat);
        }
    }

    public int RemoveModifiersBySource(object source)
    {
        if (source == null) return 0;
        int removed = 0;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (ReferenceEquals(modifiers[i].Source, source))
            {
                var stat = modifiers[i].Stat;
                modifiers.RemoveAt(i);
                OnStatChanged?.Invoke(stat);
                removed++;
            }
        }
        return removed;
    }

    public IReadOnlyList<kangtoe99_IStatModifier> GetModifiers() => modifiers;
}
