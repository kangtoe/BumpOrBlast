using System;
using System.Collections.Generic;
using UnityEngine;

public class kangtoe99_PlayerStats : MonoBehaviour
{
    [Serializable]
    public struct BaseStatOverride
    {
        public kangtoe99_StatType stat;
        public float value;
    }

    [Header("Base Stat Overrides (인스펙터에서 기본값 덮어쓰기)")]
    [SerializeField] private List<BaseStatOverride> overrides = new List<BaseStatOverride>();

    private static readonly Dictionary<kangtoe99_StatType, float> Defaults = new Dictionary<kangtoe99_StatType, float>
    {
        { kangtoe99_StatType.ProjectileCount, 1f },
        { kangtoe99_StatType.ProjectileSpeed, 20f },
        { kangtoe99_StatType.ProjectileScale, 1f },
        { kangtoe99_StatType.ProjectileSpread, 0f },
        { kangtoe99_StatType.Pierce, 0f },

        { kangtoe99_StatType.Damage, 10f },
        { kangtoe99_StatType.FireRate, 0.35f },
        { kangtoe99_StatType.EnergyCost, 1f },

        { kangtoe99_StatType.EnergyMax, 10f },
        { kangtoe99_StatType.EnergyRegen, 5f },

        { kangtoe99_StatType.MaxHP, 100f },
        { kangtoe99_StatType.HPRegen, 0f },
        { kangtoe99_StatType.BodyScale, 1f },

        { kangtoe99_StatType.MoveForce, 50f },
        { kangtoe99_StatType.RotationSpeed, 270f },
        { kangtoe99_StatType.Friction, 1f },

        { kangtoe99_StatType.Luck, 0f },
        { kangtoe99_StatType.Magnet, 1.5f }
    };

    private Dictionary<kangtoe99_StatType, float> baseValues;
    private List<kangtoe99_IStatModifier> modifiers = new List<kangtoe99_IStatModifier>();

    public event Action<kangtoe99_StatType> OnStatChanged;

    private void Awake()
    {
        baseValues = new Dictionary<kangtoe99_StatType, float>(Defaults);
        foreach (var o in overrides)
        {
            baseValues[o.stat] = o.value;
        }
    }

    public float GetBase(kangtoe99_StatType stat)
    {
        return baseValues != null && baseValues.TryGetValue(stat, out float v) ? v : 0f;
    }

    public void SetBase(kangtoe99_StatType stat, float value)
    {
        if (baseValues == null) return;
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
