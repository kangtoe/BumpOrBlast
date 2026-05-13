using System;
using System.Collections.Generic;
using UnityEngine;

public class kangtoe99_PlayerStats : MonoBehaviour
{
    [Header("Base Stat Profile (SO)")]
    [SerializeField] private kangtoe99_PlayerStatsData baseStatProfile;

    // SO 없을 때만 사용하는 코드 fallback. 일반적으로 PlayerStatsData_Default.asset 할당 권장.
    public static readonly Dictionary<kangtoe99_StatType, float> Defaults = new Dictionary<kangtoe99_StatType, float>
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
        EnsureInitialized();
    }

    // Player.Awake 등 다른 컴포넌트가 PlayerStats.Awake보다 먼저 GetFinal/GetBase를 호출하는 경우를 대비한 lazy init.
    private void EnsureInitialized()
    {
        if (baseValues != null) return;

        baseValues = new Dictionary<kangtoe99_StatType, float>(Defaults);

        if (baseStatProfile != null)
        {
            foreach (var entry in baseStatProfile.BaseStats)
            {
                baseValues[entry.stat] = entry.value;
            }
        }
        else
        {
            Debug.LogWarning("[kangtoe99_PlayerStats] baseStatProfile(SO)이 할당되지 않아 코드 Defaults를 사용합니다. PlayerStatsData 자산을 할당하세요.");
        }
    }

    public float GetBase(kangtoe99_StatType stat)
    {
        EnsureInitialized();
        return baseValues.TryGetValue(stat, out float v) ? v : 0f;
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
