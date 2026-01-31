using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class kangtoe99_LevelUpSystem : MonoBehaviour
{
    public static kangtoe99_LevelUpSystem Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int baseScore = 100;
    [SerializeField] private int scorePerLevel = 50;
    private int currentLevel = 1;
    private int nextLevelScore;
    private int previousLevelScore = 0;

    [Header("Upgrade Options")]
    [SerializeField] private kangtoe99_Player player;
    [SerializeField] private kangtoe99_PlayerShooting playerShooting;
    private UpgradeType? selectedUpgrade = null;

    // 각 업그레이드별 레벨 추적
    private System.Collections.Generic.Dictionary<UpgradeType, int> upgradeLevels;

    [Header("Upgrade Values (Fixed Amounts)")]
    [SerializeField] private float damageIncrement = 5f;
    [SerializeField] private float speedIncrement = 1f;
    [SerializeField] private int ammoIncrement = 2;
    [SerializeField] private float knockbackIncrement = 2f; // 넉백 증가량

    [Header("UI")]
    [SerializeField] private Image expBar;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Button damageButton;
    [SerializeField] private Button speedButton;
    [SerializeField] private Button ammoButton;
    [SerializeField] private Button knockbackButton;
    [SerializeField] private Button confirmButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기 레벨 설정
        nextLevelScore = baseScore;

        // 업그레이드 레벨 초기화
        upgradeLevels = new System.Collections.Generic.Dictionary<UpgradeType, int>
        {
            { UpgradeType.IncreaseDamage, 0 },
            { UpgradeType.IncreaseSpeed, 0 },
            { UpgradeType.IncreaseMaxAmmo, 0 },
            { UpgradeType.IncreaseKnockback, 0 }
        };

        // Player 참조 찾기
        if (player == null)
        {
            player = FindFirstObjectByType<kangtoe99_Player>();
        }
        if (playerShooting == null)
        {
            playerShooting = FindFirstObjectByType<kangtoe99_PlayerShooting>();
        }

        // 패널 숨기기
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // 버튼 이벤트 연결
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseDamage));
        }
        if (speedButton != null)
        {
            speedButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseSpeed));
        }
        if (ammoButton != null)
        {
            ammoButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseMaxAmmo));
        }
        if (knockbackButton != null)
        {
            knockbackButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseKnockback));
        }
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmUpgrade);
            SetButtonText(confirmButton, "Confirm");
        }
    }

    private void SetButtonText(Button button, string text)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    private void Update()
    {
        CheckLevelUp();
        UpdateExpBar();
    }

    private void UpdateExpBar()
    {
        if (expBar == null || kangtoe99_ScoreSystem.Instance == null) return;

        int currentScore = kangtoe99_ScoreSystem.Instance.GetCurrentScore();
        float progress = (float)(currentScore - previousLevelScore) / (nextLevelScore - previousLevelScore);
        expBar.fillAmount = Mathf.Clamp01(progress);
    }

    private void CheckLevelUp()
    {
        if (kangtoe99_ScoreSystem.Instance == null) return;

        int currentScore = kangtoe99_ScoreSystem.Instance.GetCurrentScore();

        if (currentScore >= nextLevelScore)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        previousLevelScore = nextLevelScore;

        // 레벨이 올라갈수록 필요한 점수가 증가
        // Level 1→2: baseScore (100)
        // Level 2→3: baseScore + scorePerLevel (150)
        // Level 3→4: baseScore + 2*scorePerLevel (200)
        int requiredScore = baseScore + (currentLevel - 1) * scorePerLevel;
        nextLevelScore = previousLevelScore + requiredScore;

        Debug.Log($"Level Up! Now Level {currentLevel}");

        // 게임 일시 정지
        Time.timeScale = 0f;

        // 레벨업 패널 표시
        ShowPanel();
    }

    private void ShowPanel()
    {
        if (levelUpPanel != null)
        {
            selectedUpgrade = null;
            UpdateButtonTexts();
            UpdateButtonHighlight();
            levelUpPanel.SetActive(true);
        }
    }

    private void UpdateButtonTexts()
    {
        // Damage 버튼
        if (damageButton != null && playerShooting != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseDamage];
            float current = playerShooting.GetBulletDamage();
            float upgraded = current + damageIncrement;
            SetButtonText(damageButton, $"Damage Lv.{level}\n{current:F1} → {upgraded:F1}");
        }

        // Speed 버튼
        if (speedButton != null && player != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseSpeed];
            float current = player.GetMoveSpeed();
            float upgraded = current + speedIncrement;
            SetButtonText(speedButton, $"Speed Lv.{level}\n{current:F1} → {upgraded:F1}");
        }

        // Max Ammo 버튼
        if (ammoButton != null && playerShooting != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseMaxAmmo];
            int current = playerShooting.GetMaxAmmo();
            int upgraded = current + ammoIncrement;
            SetButtonText(ammoButton, $"Max Ammo Lv.{level}\n{current} → {upgraded}");
        }

        // Knockback 버튼 (넉백)
        if (knockbackButton != null && playerShooting != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseKnockback];
            float current = playerShooting.GetBulletKnockback();
            float upgraded = current + knockbackIncrement;
            SetButtonText(knockbackButton, $"Knockback Lv.{level}\n{current:F1} → {upgraded:F1}");
        }
    }

    private void HidePanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    private void SelectUpgradeOption(UpgradeType upgradeType)
    {
        selectedUpgrade = upgradeType;
        UpdateButtonHighlight();
    }

    private void UpdateButtonHighlight()
    {
        // 모든 버튼을 기본 색상으로
        ResetButtonColor(damageButton);
        ResetButtonColor(speedButton);
        ResetButtonColor(ammoButton);
        ResetButtonColor(knockbackButton);

        // 선택된 버튼만 하이라이트
        if (selectedUpgrade.HasValue)
        {
            Button selectedButton = selectedUpgrade.Value switch
            {
                UpgradeType.IncreaseDamage => damageButton,
                UpgradeType.IncreaseSpeed => speedButton,
                UpgradeType.IncreaseMaxAmmo => ammoButton,
                UpgradeType.IncreaseKnockback => knockbackButton,
                _ => null
            };

            if (selectedButton != null)
            {
                HighlightButton(selectedButton);
            }
        }
    }

    private void ResetButtonColor(Button button)
    {
        if (button == null) return;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        button.colors = colors;
    }

    private void HighlightButton(Button button)
    {
        if (button == null) return;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.yellow;
        colors.selectedColor = Color.yellow;
        colors.pressedColor = Color.yellow;
        button.colors = colors;
    }

    private void ConfirmUpgrade()
    {
        if (!selectedUpgrade.HasValue)
        {
            Debug.LogWarning("No upgrade selected!");
            return;
        }

        ApplyUpgrade(selectedUpgrade.Value);
        selectedUpgrade = null;
        HidePanel();
    }

    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        // 업그레이드 레벨 증가
        upgradeLevels[upgradeType]++;
        int currentLevel = upgradeLevels[upgradeType];

        switch (upgradeType)
        {
            case UpgradeType.IncreaseDamage:
                // 탄환 피해량 고정값 증가
                if (playerShooting != null)
                {
                    float currentDamage = playerShooting.GetBulletDamage();
                    float newDamage = currentDamage + damageIncrement;
                    playerShooting.SetBulletDamage(newDamage);
                    Debug.Log($"Damage Lv.{currentLevel}: {currentDamage:F1} → {newDamage:F1}");
                }
                break;

            case UpgradeType.IncreaseSpeed:
                // 이동속도 고정값 증가
                if (player != null)
                {
                    float currentSpeed = player.GetMoveSpeed();
                    float newSpeed = currentSpeed + speedIncrement;
                    player.SetMoveSpeed(newSpeed);
                    Debug.Log($"Speed Lv.{currentLevel}: {currentSpeed:F1} → {newSpeed:F1}");
                }
                break;

            case UpgradeType.IncreaseMaxAmmo:
                // 탄창 크기 고정값 증가
                if (playerShooting != null)
                {
                    playerShooting.IncreaseMaxAmmo(ammoIncrement);
                    Debug.Log($"Max Ammo Lv.{currentLevel}: +{ammoIncrement}");
                }
                break;

            case UpgradeType.IncreaseKnockback:
                // 탄환 넉백 증가
                if (playerShooting != null)
                {
                    float currentKnockback = playerShooting.GetBulletKnockback();
                    playerShooting.IncreaseBulletKnockback(knockbackIncrement);
                    float newKnockback = playerShooting.GetBulletKnockback();
                    Debug.Log($"Knockback Lv.{currentLevel}: {currentKnockback:F1} → {newKnockback:F1}");
                }
                break;
        }

        // 게임 재개
        Time.timeScale = 1f;
    }

    public int GetCurrentLevel() => currentLevel;
    public int GetNextLevelScore() => nextLevelScore;
    public int GetUpgradeLevel(UpgradeType upgradeType) => upgradeLevels[upgradeType];
}

public enum UpgradeType
{
    IncreaseDamage,
    IncreaseSpeed,
    IncreaseMaxAmmo,
    IncreaseKnockback
}
