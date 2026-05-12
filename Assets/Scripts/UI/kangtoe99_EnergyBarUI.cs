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
    [Tooltip("몇 포인트마다 눈금을 표시할지. 1이면 1포인트마다, 5면 5포인트마다.")]
    [SerializeField, Min(1)] private int pointsPerTick = 1;

    private int lastTotalPoints = -1;
    private int lastPointsPerTick = -1;

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

    private void Update()
    {
        // 인스펙터에서 pointsPerTick 조절 시 다음 프레임에 즉시 반영
        if (pointsPerTick != lastPointsPerTick && energySystem != null)
        {
            RebuildIfNeeded(energySystem.Max);
        }
    }

    private void OnValidate()
    {
        if (pointsPerTick < 1) pointsPerTick = 1;
    }

    private void UpdateUI(float currentValue, float maxValue)
    {
        if (fillImage != null && maxValue > 0f)
        {
            float ratio = Mathf.Clamp01(currentValue / maxValue);
            fillImage.fillAmount = ratio;
            fillImage.color = ratio <= emptyThreshold ? emptyColor : normalColor;
        }

        RebuildIfNeeded(maxValue);
    }

    private void RebuildIfNeeded(float maxValue)
    {
        int totalPoints = Mathf.Max(1, Mathf.RoundToInt(maxValue));
        int ppt = Mathf.Max(1, pointsPerTick);

        if (totalPoints != lastTotalPoints || ppt != lastPointsPerTick)
        {
            RebuildTicks(totalPoints, ppt);
            lastTotalPoints = totalPoints;
            lastPointsPerTick = ppt;
        }
    }

    // 칸 N개를 pointsPerTick 간격의 세로 분할선으로 표현 (좌우 끝은 배경 경계로 마감)
    private void RebuildTicks(int totalPoints, int ppt)
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

        if (totalPoints <= 1 || ppt < 1) return;

        for (int i = 1; i * ppt < totalPoints; i++)
        {
            int position = i * ppt;
            var tick = new GameObject($"Tick_{position}", typeof(RectTransform), typeof(Image));
            tick.transform.SetParent(tickContainer, false);

            var rt = (RectTransform)tick.transform;
            float xRatio = (float)position / totalPoints;
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
