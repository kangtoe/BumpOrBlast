using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// 아이템 UI 틀. 빌드 화면 슬롯으로 배치.
// 평소 표시: 아이콘 + 중복 수(xN). 호버 시: 독립 툴팁(kangtoe99_ItemTooltip) 에 표시 요청.
public class kangtoe99_ItemDisplayView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot")]
    [SerializeField] private Image backgroundImage; // 등급 색으로 칠해짐
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackText;

    [Header("Tier")]
    [SerializeField] private kangtoe99_TierColorPalette tierPalette; // 적·아이템 공용 등급 색상

    private kangtoe99_ItemData boundData;

    public void Bind(kangtoe99_ItemData data, int stack)
    {
        boundData = data;
        if (iconImage != null)
        {
            iconImage.sprite = data?.Icon;
            iconImage.enabled = data?.Icon != null;
        }
        if (stackText != null) stackText.text = stack > 1 ? $"x{stack}" : string.Empty;

        // 배경을 등급 색으로 — 빈 슬롯(data=null)은 가장 낮은 등급(Gray) 색으로
        if (backgroundImage != null && tierPalette != null)
        {
            var tier = data != null ? data.Tier : kangtoe99_Tier.Gray;
            backgroundImage.color = tierPalette.Get(tier);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boundData == null) return;
        var tt = kangtoe99_ItemTooltip.Instance;
        if (tt != null) tt.Show(boundData, (RectTransform)transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var tt = kangtoe99_ItemTooltip.Instance;
        if (tt != null) tt.Hide();
    }

    private void OnDisable()
    {
        var tt = kangtoe99_ItemTooltip.Instance;
        if (tt != null) tt.Hide();
    }
}
