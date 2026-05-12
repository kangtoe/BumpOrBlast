using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private kangtoe99_PlayerStats playerStats;
    private UpgradeType? selectedUpgrade = null;

    private System.Collections.Generic.Dictionary<UpgradeType, int> upgradeLevels;

    [Header("Upgrade Values (Percent Increase)")]
    private const float UpgradePercentIncrease = 0.20f;

    private float baseBulletDamage;
    private float baseFireRate;

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
    [SerializeField] private Button confirmButton;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

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

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        nextLevelScore = baseScore;

        upgradeLevels = new System.Collections.Generic.Dictionary<UpgradeType, int>
        {
            { UpgradeType.IncreaseDamage, 0 },
            { UpgradeType.IncreaseFireRate, 0 }
        };

        if (player == null)
        {
            player = FindFirstObjectByType<kangtoe99_Player>();
        }
        if (playerStats == null && player != null)
        {
            playerStats = player.Stats;
        }
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<kangtoe99_PlayerStats>();
        }

        if (playerStats != null)
        {
            baseBulletDamage = playerStats.GetBase(kangtoe99_StatType.Damage);
            baseFireRate = playerStats.GetBase(kangtoe99_StatType.FireRate);
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        UpdateLevelText();

        if (damageButton != null)
        {
            damageButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseDamage));
        }
        if (fireRateButton != null)
        {
            fireRateButton.onClick.AddListener(() => SelectUpgradeOption(UpgradeType.IncreaseFireRate));
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

        int requiredScore = baseScore + (currentLevel - 1) * scorePerLevel;
        nextLevelScore = previousLevelScore + requiredScore;

        Debug.Log($"Level Up! Now Level {currentLevel}");

        UpdateLevelText();

        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        Time.timeScale = 0f;
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
        if (damageButton != null && playerStats != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseDamage];
            float current = baseBulletDamage * (1f + level * UpgradePercentIncrease);
            float upgraded = baseBulletDamage * (1f + (level + 1) * UpgradePercentIncrease);
            SetButtonText(damageButton, $"Damage Lv.{level}\n{current:F1} > {upgraded:F1}");
        }

        if (fireRateButton != null && playerStats != null)
        {
            int level = upgradeLevels[UpgradeType.IncreaseFireRate];
            float current = baseFireRate / (1f + level * UpgradePercentIncrease);
            float upgraded = baseFireRate / (1f + (level + 1) * UpgradePercentIncrease);
            SetButtonText(fireRateButton, $"Fire Rate Lv.{level}\n{current:F2}s > {upgraded:F2}s");
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
        ResetButtonColor(damageButton);
        ResetButtonColor(fireRateButton);

        if (selectedUpgrade.HasValue)
        {
            Button selectedButton = selectedUpgrade.Value switch
            {
                UpgradeType.IncreaseDamage => damageButton,
                UpgradeType.IncreaseFireRate => fireRateButton,
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
        colors.normalColor = defaultColor;
        colors.selectedColor = defaultColor;
        colors.pressedColor = defaultColor;
        button.colors = colors;

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = defaultColor;
        }
    }

    private void HighlightButton(Button button)
    {
        if (button == null) return;
        ColorBlock colors = button.colors;
        colors.normalColor = selectedColor;
        colors.selectedColor = selectedColor;
        colors.pressedColor = selectedColor;
        button.colors = colors;

        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = selectedColor;
        }
    }

    private void ConfirmUpgrade()
    {
        if (!selectedUpgrade.HasValue)
        {
            Debug.LogWarning("No upgrade selected!");
            return;
        }

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
        upgradeLevels[upgradeType]++;
        int level = upgradeLevels[upgradeType];

        switch (upgradeType)
        {
            case UpgradeType.IncreaseDamage:
                if (playerStats != null)
                {
                    float newDamage = baseBulletDamage * (1f + level * UpgradePercentIncrease);
                    playerStats.SetBase(kangtoe99_StatType.Damage, newDamage);
                    Debug.Log($"Damage Lv.{level}: → {newDamage:F1}");
                }
                break;

            case UpgradeType.IncreaseFireRate:
                if (playerStats != null)
                {
                    float newFireRate = baseFireRate / (1f + level * UpgradePercentIncrease);
                    playerStats.SetBase(kangtoe99_StatType.FireRate, newFireRate);
                    Debug.Log($"Fire Rate Lv.{level}: → {newFireRate:F2}s");
                }
                break;
        }

        Time.timeScale = 1f;
    }

    public int GetCurrentLevel() => currentLevel;
    public int GetNextLevelScore() => nextLevelScore;
    public int GetUpgradeLevel(UpgradeType upgradeType) => upgradeLevels[upgradeType];
}

public enum UpgradeType
{
    IncreaseDamage,
    IncreaseFireRate
}
