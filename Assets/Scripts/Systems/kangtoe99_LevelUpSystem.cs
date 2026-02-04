using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class kangtoe99_LevelUpSystem : MonoBehaviour
{
    public static kangtoe99_LevelUpSystem Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int baseScore = 100;
    [SerializeField] private int scorePerLevel = 50;
    private int currentLevel = 0;
    private int nextLevelScore;
    private int previousLevelScore = 0;

    [Header("Upgrade Options")]
    [SerializeField] private kangtoe99_Player player;
    [SerializeField] private kangtoe99_PlayerShooting playerShooting;
    private UpgradeType? selectedUpgrade = null;

    // 각 업그레이드별 레벨 추적
    private System.Collections.Generic.Dictionary<UpgradeType, int> upgradeLevels;

    [Header("Upgrade Values (Percent Increase)")]
    private const float UpgradePercentIncrease = 0.20f; // 모든 업그레이드 20% 증가

    // 기본값 저장 (선형 증가 계산용)
    private float baseBulletDamage;
    private float baseFireRate;
    private int baseMaxAmmo;
    private float baseReloadTime;

    [Header("SFX")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip upgradeSound;
    private AudioSource audioSource;

    [Header("UI")]
    [SerializeField] private Image expBar;
    [SerializeField] private Text levelText;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Button damageButton;
    [SerializeField] private Button fireRateButton;
    [SerializeField] private Button ammoButton;
    [SerializeField] private Button reloadButton;
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

        // AudioSource 설정
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // 초기 레벨 설정
        nextLevelScore = baseScore;

        // 업그레이드 레벨 초기화
        upgradeLevels = new System.Collections.Generic.Dictionary<UpgradeType, int>
        {
            { UpgradeType.IncreaseDamage, 0 },
            { UpgradeType.IncreaseFireRate, 0 },
            { UpgradeType.IncreaseMaxAmmo, 0 },
            { UpgradeType.IncreaseReloadSpeed, 0 }
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

        // 기본값 저장 (선형 증가 계산용)
        if (playerShooting != null)
        {
            baseBulletDamage = playerShooting.GetBulletDamage();
            baseFireRate = playerShooting.GetFireRate();
            baseMaxAmmo = playerShooting.GetMaxAmmo();
            baseReloadTime = playerShooting.GetReloadTime();
        }

        // 패널 숨기기
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // 초기 레벨 텍스트 업데이트
        UpdateLevelText();

        // 버튼 이벤트 연결
        if (damageButton != null)
        {
            damageButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseDamage));
        }
        if (fireRateButton != null)
        {
            fireRateButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseFireRate));
        }
        if (ammoButton != null)
        {
            ammoButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseMaxAmmo));
        }
        if (reloadButton != null)
        {
            reloadButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseReloadSpeed));
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

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"Lv.{currentLevel}";
        }
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

        // 레벨 텍스트 업데이트
        UpdateLevelText();

        // 레벨 업 사운드 재생
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

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
        // Damage 버튼 (데미지 증가)
        if (damageButton != null && playerShooting != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseDamage];
            float current = baseBulletDamage * (1f + level * UpgradePercentIncrease);
            float upgraded = baseBulletDamage * (1f + (level + 1) * UpgradePercentIncrease);
            SetButtonText(damageButton, $"Damage Lv.{level}\n{current:F1} > {upgraded:F1}");
        }

        // Fire Rate 버튼 (연사속도 증가 = 발사 간격 감소, 속도 기반 계산)
        if (fireRateButton != null && playerShooting != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseFireRate];
            float current = baseFireRate / (1f + level * UpgradePercentIncrease);
            float upgraded = baseFireRate / (1f + (level + 1) * UpgradePercentIncrease);
            SetButtonText(fireRateButton, $"Fire Rate Lv.{level}\n{current:F2}s > {upgraded:F2}s");
        }

        // Max Ammo 버튼 (탄창 크기 증가)
        if (ammoButton != null && playerShooting != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseMaxAmmo];
            int current = Mathf.RoundToInt(baseMaxAmmo * (1f + level * UpgradePercentIncrease));
            int upgraded = Mathf.RoundToInt(baseMaxAmmo * (1f + (level + 1) * UpgradePercentIncrease));
            SetButtonText(ammoButton, $"Max Ammo Lv.{level}\n{current} > {upgraded}");
        }

        // Reload Speed 버튼 (재장전 속도 증가 = 재장전 시간 감소, 속도 기반 계산)
        if (reloadButton != null && playerShooting != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseReloadSpeed];
            float current = baseReloadTime / (1f + level * UpgradePercentIncrease);
            float upgraded = baseReloadTime / (1f + (level + 1) * UpgradePercentIncrease);
            SetButtonText(reloadButton, $"Reload Lv.{level}\n{current:F2}s > {upgraded:F2}s");
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
        ResetButtonColor(fireRateButton);
        ResetButtonColor(ammoButton);
        ResetButtonColor(reloadButton);

        // 선택된 버튼만 하이라이트
        if (selectedUpgrade.HasValue)
        {
            Button selectedButton = selectedUpgrade.Value switch
            {
                UpgradeType.IncreaseDamage => damageButton,
                UpgradeType.IncreaseFireRate => fireRateButton,
                UpgradeType.IncreaseMaxAmmo => ammoButton,
                UpgradeType.IncreaseReloadSpeed => reloadButton,
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
        colors.selectedColor = Color.white;
        colors.pressedColor = Color.white;
        button.colors = colors;
    }

    private void HighlightButton(Button button)
    {
        if (button == null) return;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.green;
        colors.selectedColor = Color.green;
        colors.pressedColor = Color.green;
        button.colors = colors;
    }

    private void ConfirmUpgrade()
    {
        if (!selectedUpgrade.HasValue)
        {
            Debug.LogWarning("No upgrade selected!");
            return;
        }

        // 업그레이드 사운드 재생
        if (upgradeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(upgradeSound);
        }

        ApplyUpgrade(selectedUpgrade.Value);
        selectedUpgrade = null;
        HidePanel();
    }

    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        // 업그레이드 레벨 증가
        upgradeLevels[upgradeType]++;
        int level = upgradeLevels[upgradeType];

        switch (upgradeType)
        {
            case UpgradeType.IncreaseDamage:
                // 탄환 피해량: 기본값 * (1 + 레벨 * 증가율)
                if (playerShooting != null)
                {
                    float prevDamage = baseBulletDamage * (1f + (level - 1) * UpgradePercentIncrease);
                    float newDamage = baseBulletDamage * (1f + level * UpgradePercentIncrease);
                    playerShooting.SetBulletDamage(newDamage);
                    Debug.Log($"Damage Lv.{level}: {prevDamage:F1} → {newDamage:F1} (+{UpgradePercentIncrease * 100f:F0}%)");
                }
                break;

            case UpgradeType.IncreaseFireRate:
                // 연사속도: 기본값 / (1 + 레벨 * 증가율) - 속도 기반 계산
                if (playerShooting != null)
                {
                    float prevFireRate = baseFireRate / (1f + (level - 1) * UpgradePercentIncrease);
                    float newFireRate = baseFireRate / (1f + level * UpgradePercentIncrease);
                    playerShooting.SetFireRate(newFireRate);
                    Debug.Log($"Fire Rate Lv.{level}: {prevFireRate:F2}s → {newFireRate:F2}s (+{UpgradePercentIncrease * 100f:F0}%)");
                }
                break;

            case UpgradeType.IncreaseMaxAmmo:
                // 탄창 크기: 기본값 * (1 + 레벨 * 증가율)
                if (playerShooting != null)
                {
                    int prevAmmo = Mathf.RoundToInt(baseMaxAmmo * (1f + (level - 1) * UpgradePercentIncrease));
                    int newAmmo = Mathf.RoundToInt(baseMaxAmmo * (1f + level * UpgradePercentIncrease));
                    int ammoIncrease = newAmmo - playerShooting.GetMaxAmmo();
                    playerShooting.IncreaseMaxAmmo(ammoIncrease);
                    Debug.Log($"Max Ammo Lv.{level}: {prevAmmo} → {newAmmo} (+{UpgradePercentIncrease * 100f:F0}%)");
                }
                break;

            case UpgradeType.IncreaseReloadSpeed:
                // 재장전 속도: 기본값 / (1 + 레벨 * 증가율) - 속도 기반 계산
                if (playerShooting != null)
                {
                    float prevReload = baseReloadTime / (1f + (level - 1) * UpgradePercentIncrease);
                    float newReload = baseReloadTime / (1f + level * UpgradePercentIncrease);
                    playerShooting.SetReloadTime(newReload);
                    Debug.Log($"Reload Lv.{level}: {prevReload:F2}s → {newReload:F2}s (+{UpgradePercentIncrease * 100f:F0}%)");
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
    IncreaseDamage,      // 데미지 증가
    IncreaseFireRate,    // 연사속도 증가
    IncreaseMaxAmmo,     // 탄창 크기 증가
    IncreaseReloadSpeed  // 재장전 속도 증가
}
