using UnityEngine;
using UnityEngine.UI;

public class kangtoe99_EnergyBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private kangtoe99_EnergySystem energySystem;
    [SerializeField] private Image fillImage; // 눈금은 이 fillImage 기준으로 배치된다 (배경 아님)

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
            // fill 폭이 아직 0(레이아웃 미계산)이면 RebuildTicks가 false — 다음 프레임에 재시도
            if (RebuildTicks(totalPoints, ppt))
            {
                lastTotalPoints = totalPoints;
                lastPointsPerTick = ppt;
            }
        }
    }

    // 칸 N개를 pointsPerTick 간격의 세로 분할선으로 표현.
    // 위치 기준은 배경이 아니라 fillImage — fill의 실제 폭을 기준으로 한다.
    // 눈금이 차지하는 너비를 빼고 칸을 균등 분할 — 양끝 칸도 내부 칸과 같은 폭이 된다.
    // fill 폭이 아직 0이면(레이아웃 미계산) false 반환 → 호출부가 다음 프레임에 재시도.
    private bool RebuildTicks(int totalPoints, int ppt)
    {
        if (fillImage == null) return false;
        RectTransform fillRt = fillImage.rectTransform;

        // 기존 눈금 제거 (이 스크립트가 만든 Tick_* 만)
        for (int i = fillRt.childCount - 1; i >= 0; i--)
        {
            var child = fillRt.GetChild(i).gameObject;
            if (!child.name.StartsWith("Tick_")) continue;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        if (totalPoints <= 1 || ppt < 1) return true;

        float fillWidth = fillRt.rect.width;
        if (fillWidth <= 0f) return false; // 레이아웃 아직 — 재시도

        // 칸 N개 균등 분할: 눈금이 차지하는 총 너비를 빼고 남은 폭을 totalPoints로 나눈다.
        // 눈금은 칸과 칸 "사이"를 차지하므로 양끝 칸도 내부 칸과 폭이 같아진다.
        int numTicks = Mathf.CeilToInt((float)totalPoints / ppt) - 1;
        float cellWidth = (fillWidth - numTicks * tickWidth) / totalPoints;

        for (int i = 1; i * ppt < totalPoints; i++)
        {
            int position = i * ppt;
            var tick = new GameObject($"Tick_{position}", typeof(RectTransform), typeof(Image));
            tick.transform.SetParent(fillRt, false);

            var rt = (RectTransform)tick.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(tickWidth, 0f);
            // 이 눈금 왼쪽 = 앞 칸 position개 + 앞 눈금 (i-1)개. 중심은 거기서 + tickWidth/2.
            rt.anchoredPosition = new Vector2(
                -fillWidth * 0.5f + position * cellWidth + (i - 0.5f) * tickWidth, 0f);

            var img = tick.GetComponent<Image>();
            img.color = tickColor;
            img.raycastTarget = false;
        }
        return true;
    }
}
