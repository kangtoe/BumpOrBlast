using System;
using UnityEngine;
using UnityEngine.UI;

// LevelUp 패널 한 슬롯. Button + 공통 ItemDisplay 인스턴스.
// 시각은 ItemDisplayView가 SO 데이터를 받아 채움 — 슬롯 자체는 클릭·레이아웃만 담당.
public class kangtoe99_LevelUpChoiceSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private RectTransform displayContainer;
    [SerializeField] private kangtoe99_ItemDisplayView displayPrefab;

    private kangtoe99_ILevelUpChoice choice;
    private Action<kangtoe99_ILevelUpChoice> onSelected;
    private kangtoe99_ItemDisplayView currentDisplay;

    public void Bind(kangtoe99_ILevelUpChoice c, Action<kangtoe99_ILevelUpChoice> handler)
    {
        choice = c;
        onSelected = handler;

        if (currentDisplay == null && displayPrefab != null && displayContainer != null)
        {
            currentDisplay = Instantiate(displayPrefab, displayContainer);
        }
        currentDisplay?.Bind(c, stack: 0, showName: true, showDescription: true);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        onSelected?.Invoke(choice);
    }
}
