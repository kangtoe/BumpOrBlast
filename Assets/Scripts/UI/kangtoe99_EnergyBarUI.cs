using UnityEngine;
using UnityEngine.UI;

public class kangtoe99_EnergyBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private kangtoe99_EnergySystem energySystem;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text valueText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.7f, 1f);
    [SerializeField] private Color emptyColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private float emptyThreshold = 0.15f;

    private void Start()
    {
        if (energySystem == null)
        {
            energySystem = FindFirstObjectByType<kangtoe99_EnergySystem>();
        }

        if (energySystem != null)
        {
            energySystem.OnEnergyChanged += UpdateUI;
            UpdateUI(energySystem.Current, energySystem.Max);
        }
        else
        {
            Debug.LogWarning("[kangtoe99_EnergyBarUI] kangtoe99_EnergySystem을 찾지 못했습니다.");
        }
    }

    private void OnDestroy()
    {
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged -= UpdateUI;
        }
    }

    private void UpdateUI(float currentValue, float maxValue)
    {
        if (fillImage != null && maxValue > 0f)
        {
            float ratio = Mathf.Clamp01(currentValue / maxValue);
            fillImage.fillAmount = ratio;
            fillImage.color = ratio <= emptyThreshold ? emptyColor : normalColor;
        }

        if (valueText != null)
        {
            valueText.text = $"{Mathf.FloorToInt(currentValue)}/{Mathf.FloorToInt(maxValue)}";
        }
    }
}
