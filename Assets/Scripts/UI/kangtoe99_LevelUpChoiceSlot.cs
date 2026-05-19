using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 선택지 UI 틀. LevelUp 패널에 4개 인스턴스화.
// 자체적으로 Icon + Name + Description을 직접 표시. ItemDisplayView 사용 안 함.
public class kangtoe99_LevelUpChoiceSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage; // 등급 색으로 칠해짐 (= 버튼 배경)
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText; // 레어도 라벨 — 등급 없는 선택지(InstantDrop 등)면 비활성
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private ScrollRect descriptionScroll; // 설명이 영역을 넘으면 스크롤 — Bind 시 최상단으로 리셋

    [Header("Tier")]
    [SerializeField] private kangtoe99_TierColorPalette tierPalette; // 적·아이템 공용 등급 색상
    [Tooltip("ItemData가 아닌 선택지(InstantDrop 등) 배경색 — 등급이 없음")]
    [SerializeField] private Color nonTierColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);

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

        // 배경을 등급 색으로 — ItemData만 등급이 있고, 그 외(InstantDrop 등)는 nonTierColor
        bool hasTier = c is kangtoe99_ItemData;
        if (backgroundImage != null)
        {
            backgroundImage.color = (hasTier && tierPalette != null)
                ? tierPalette.Get(((kangtoe99_ItemData)c).Tier)
                : nonTierColor;
        }

        // 레어도 라벨 — Tier 없는 선택지는 숨김
        if (rarityText != null)
        {
            rarityText.gameObject.SetActive(hasTier);
            if (hasTier) rarityText.text = kangtoe99_TierNames.GetDisplayName(((kangtoe99_ItemData)c).Tier);
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        // 새 선택지 표시 시 스크롤 위치 리셋 — 이전 선택지의 스크롤 잔재 방지
        if (descriptionScroll != null) descriptionScroll.verticalNormalizedPosition = 1f;
    }

    private void OnClicked() => onSelected?.Invoke(choice);
}
