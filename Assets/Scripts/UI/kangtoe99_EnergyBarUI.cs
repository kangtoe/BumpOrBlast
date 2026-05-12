using UnityEngine;
using UnityEngine.UI;

public class kangtoe99_EnergyBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private kangtoe99_EnergySystem energySystem;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform tickContainer;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.7f, 1f);
    [SerializeField] private Color emptyColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private float emptyThreshold = 0.15f;

    [Header("Tick Settings")]
    [SerializeField] private Color tickColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private float tickWidth = 2f;

    private int lastTickCount = -1;

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

        int tickCount = Mathf.Max(1, Mathf.RoundToInt(maxValue));
        if (tickCount != lastTickCount)
        {
            RebuildTicks(tickCount);
            lastTickCount = tickCount;
        }
    }

    // 칸 N개를 N-1개의 세로 분할선으로 표현 (좌우 끝은 배경 경계로 자연스럽게 마감)
    private void RebuildTicks(int count)
    {
        if (tickContainer == null) return;

        for (int i = tickContainer.childCount - 1; i >= 0; i--)
        {
            var child = tickContainer.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        if (count <= 1) return;

        for (int i = 1; i < count; i++)
        {
            var tick = new GameObject($"Tick_{i}", typeof(RectTransform), typeof(Image));
            tick.transform.SetParent(tickContainer, false);

            var rt = (RectTransform)tick.transform;
            float xRatio = (float)i / count;
            rt.anchorMin = new Vector2(xRatio, 0f);
            rt.anchorMax = new Vector2(xRatio, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(tickWidth, 0f);

            var img = tick.GetComponent<Image>();
            img.color = tickColor;
            img.raycastTarget = false;
        }
    }
}
