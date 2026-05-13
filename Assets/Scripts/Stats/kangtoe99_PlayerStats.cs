using System;
using System.Collections.Generic;
using UnityEngine;

public class kangtoe99_PlayerStats : MonoBehaviour
{
    [Header("Base Stat Profile (SO)")]
    [SerializeField] private kangtoe99_PlayerStatsData baseStatProfile;

    // 코드 fallback 기준값. SO 자산이 비어 있거나 미할당일 때 사용.
    public static readonly kangtoe99_StatMap Defaults = new kangtoe99_StatMap(GetDefaultFor);

    private kangtoe99_StatMap baseValues;
    private List<kangtoe99_IStatModifier> modifiers = new List<kangtoe99_IStatModifier>();

    public event Action<kangtoe99_StatType> OnStatChanged;

    private static float GetDefaultFor(kangtoe99_StatType stat)
    {
        switch (stat)
        {
            case kangtoe99_StatType.ProjectileCount: return 1f;
            case kangtoe99_StatType.ProjectileSpeed: return 20f;
            case kangtoe99_StatType.ProjectileScale: return 1f;
            case kangtoe99_StatType.ProjectileSpread: return 0f;
            case kangtoe99_StatType.Pierce: return 0f;

            case kangtoe99_StatType.Damage: return 10f;
            case kangtoe99_StatType.FireRate: return 0.35f;
            case kangtoe99_StatType.EnergyCost: return 1f;

            case kangtoe99_StatType.EnergyMax: return 10f;
            case kangtoe99_StatType.EnergyRegen: return 5f;

            case kangtoe99_StatType.MaxHP: return 100f;
            case kangtoe99_StatType.HPRegen: return 0f;
            case kangtoe99_StatType.BodyScale: return 1f;

            case kangtoe99_StatType.MoveForce: return 50f;
            case kangtoe99_StatType.RotationSpeed: return 270f;
            case kangtoe99_StatType.Friction: return 1f;

            case kangtoe99_StatType.Luck: return 0f;
            case kangtoe99_StatType.Magnet: return 1.5f;

            default: return 0f;
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (baseValues != null) return;

        baseValues = new kangtoe99_StatMap();
        baseValues.CopyFrom(Defaults);

        if (baseStatProfile != null)
        {
            baseValues.CopyFrom(baseStatProfile.BaseStats);
        }
        else
        {
            Debug.LogWarning("[kangtoe99_PlayerStats] baseStatProfile(SO)이 할당되지 않아 코드 Defaults를 사용합니다.");
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
