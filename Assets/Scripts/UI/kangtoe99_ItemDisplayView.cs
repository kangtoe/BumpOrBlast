using UnityEngine;
using UnityEngine.UI;

// 공통 아이템 시각 prefab의 root에 부착되는 컴포넌트.
// LevelUpChoiceSlot · BuildEntrySlot이 한 인스턴스를 자식으로 생성한 뒤 Bind 호출.
// SO(ItemData / InstantDropItemData)의 Icon · displayName · Description을 주입받아 표시.
public class kangtoe99_ItemDisplayView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text stackText;

    public void Bind(kangtoe99_ILevelUpChoice choice, int stack = 0, bool showName = true, bool showDescription = true)
    {
        if (iconImage != null)
        {
            iconImage.sprite = choice?.Icon;
            iconImage.enabled = choice?.Icon != null;
        }
        if (stackText != null)
        {
            stackText.text = stack > 1 ? $"x{stack}" : string.Empty;
        }
        if (nameText != null)
        {
            nameText.gameObject.SetActive(showName);
            if (showName) nameText.text = choice?.DisplayName ?? string.Empty;
        }
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(showDescription);
            if (showDescription) descriptionText.text = choice?.Description ?? string.Empty;
        }
    }
}
