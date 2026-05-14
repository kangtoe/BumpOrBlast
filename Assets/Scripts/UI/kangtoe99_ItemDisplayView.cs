using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// 아이템 UI 틀. 빌드 화면 슬롯으로 배치.
// 평소 표시: 아이콘 + 중복 수(xN).
// 마우스 호버 시: 자식 tooltipRoot 활성화 → 이름/설명 표시.
public class kangtoe99_ItemDisplayView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackText;

    [Header("Tooltip (hover)")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private TMP_Text tooltipNameText;
    [SerializeField] private TMP_Text tooltipDescriptionText;

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

        // 툴팁 내용 미리 채워두고, 시작은 숨김
        if (tooltipNameText != null) tooltipNameText.text = data?.DisplayName ?? string.Empty;
        if (tooltipDescriptionText != null) tooltipDescriptionText.text = data?.Description ?? string.Empty;
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipRoot != null && boundData != null) tooltipRoot.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }
}
