using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 선택지 UI 틀. LevelUp 패널에 4개 인스턴스화.
// 자체적으로 Icon + Name + Description을 직접 표시. ItemDisplayView 사용 안 함.
public class kangtoe99_LevelUpChoiceSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    private kangtoe99_ILevelUpChoice choice;
    private Action<kangtoe99_ILevelUpChoice> onSelected;

    public void Bind(kangtoe99_ILevelUpChoice c, Action<kangtoe99_ILevelUpChoice> handler)
    {
        choice = c;
        onSelected = handler;

        if (iconImage != null)
        {
            iconImage.sprite = c?.Icon;
            iconImage.enabled = c?.Icon != null;
        }
        if (nameText != null) nameText.text = c?.DisplayName ?? string.Empty;
        if (descriptionText != null) descriptionText.text = c?.Description ?? string.Empty;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked() => onSelected?.Invoke(choice);
}
