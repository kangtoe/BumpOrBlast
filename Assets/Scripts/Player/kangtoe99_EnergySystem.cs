using System;
using UnityEngine;

public class kangtoe99_EnergySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private kangtoe99_PlayerStats stats;

    [Header("Fallback (stats 없을 때만 사용)")]
    [SerializeField] private float fallbackMax = 10f;
    [SerializeField] private float fallbackRegen = 5f;

    private float current;
    private bool initialized;

    public event Action<float, float> OnEnergyChanged;

    public float Current => current;
    public float Max => stats != null ? stats.GetFinal(kangtoe99_StatType.EnergyMax) : fallbackMax;
    public float Regen => stats != null ? stats.GetFinal(kangtoe99_StatType.EnergyRegen) : fallbackRegen;

    private void Awake()
    {
        if (stats == null)
        {
            stats = GetComponent<kangtoe99_PlayerStats>();
        }
        if (stats != null)
        {
            stats.OnStatChanged += OnStatChanged;
        }
    }

    private void Start()
    {
        current = Max;
        initialized = true;
        OnEnergyChanged?.Invoke(current, Max);
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnStatChanged -= OnStatChanged;
        }
    }

    private void OnStatChanged(kangtoe99_StatType stat)
    {
        if (stat == kangtoe99_StatType.EnergyMax)
        {
            current = Mathf.Min(current, Max);
            OnEnergyChanged?.Invoke(current, Max);
        }
    }

    private void Update()
    {
        if (!initialized) return;
        if (Time.timeScale == 0f) return;
        if (kangtoe99_GameManager.Instance != null && !kangtoe99_GameManager.Instance.IsGameStarted()) return;

        float max = Max;
        if (current >= max) return;

        current = Mathf.Min(max, current + Regen * Time.deltaTime);
        OnEnergyChanged?.Invoke(current, max);
    }

    public bool HasEnergy(float amount) => current >= amount;

    public bool TryConsume(float amount)
    {
        if (amount <= 0f) return true;
        if (current < amount) return false;

        current -= amount;
        OnEnergyChanged?.Invoke(current, Max);
        return true;
    }
}
