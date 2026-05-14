using UnityEngine;

// 빌드 영역 한 슬롯. 공통 ItemDisplayView를 자식으로 생성 후 SO 데이터·stack 주입.
// xN 표기는 ItemDisplayView 내부의 stackText(우상단 anchor)가 담당.
public class kangtoe99_BuildEntrySlot : MonoBehaviour
{
    [SerializeField] private RectTransform displayContainer;
    [SerializeField] private kangtoe99_ItemDisplayView displayPrefab;

    private kangtoe99_ItemDisplayView currentDisplay;

    public void Bind(kangtoe99_ItemData data, int stack, bool showName, bool showDescription)
    {
        if (currentDisplay == null && displayPrefab != null && displayContainer != null)
        {
            currentDisplay = Instantiate(displayPrefab, displayContainer);
        }
        currentDisplay?.Bind(data, stack, showName, showDescription);
    }
}
