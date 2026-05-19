using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class kangtoe99_LevelUpSystem : MonoBehaviour
{
    public static kangtoe99_LevelUpSystem Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int baseScore = 100;
    [SerializeField] private int scorePerLevel = 50;
    [SerializeField, Min(1)] private int choiceCount = 4;

    [Header("Player")]
    [SerializeField] private kangtoe99_Player player;

    [Header("Choice Pool")]
    [SerializeField] private List<kangtoe99_ItemData> itemPool = new List<kangtoe99_ItemData>();
    [Tooltip("아이템 풀이 부족할 때 슬롯을 채우는 폴백. 현재 구현 드롭 3종(XP/HP/Bomb) 권장.")]
    [SerializeField] private List<kangtoe99_InstantDropItemData> instantDropPool = new List<kangtoe99_InstantDropItemData>();

    [Header("UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private kangtoe99_LevelUpChoiceSlot slotPrefab;
    [SerializeField] private Image expBar;
    [SerializeField] private TMP_Text levelText;

    [Header("SFX")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip upgradeSound;

    private int currentLevel = 0;
    private int nextLevelScore;
    private int previousLevelScore = 0;
    private AudioSource audioSource;
    private readonly List<kangtoe99_LevelUpChoiceSlot> activeSlots = new List<kangtoe99_LevelUpChoiceSlot>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        nextLevelScore = baseScore;

        if (player == null) player = FindFirstObjectByType<kangtoe99_Player>();

        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        UpdateLevelText();
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
        if (levelText != null) levelText.text = $"Lv.{currentLevel}";
    }

    private void CheckLevelUp()
    {
        if (kangtoe99_ScoreSystem.Instance == null) return;
        int currentScore = kangtoe99_ScoreSystem.Instance.GetCurrentScore();
        if (currentScore >= nextLevelScore) LevelUp();
    }

    private void LevelUp()
    {
        currentLevel++;
        previousLevelScore = nextLevelScore;
        int requiredScore = baseScore + (currentLevel - 1) * scorePerLevel;
        nextLevelScore = previousLevelScore + requiredScore;
        UpdateLevelText();

        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        Time.timeScale = 0f;
        if (kangtoe99_GameManager.Instance != null)
            kangtoe99_GameManager.Instance.SetHudVisible(false);
        ShowChoices();
    }

    private void ShowChoices()
    {
        if (levelUpPanel == null || slotContainer == null || slotPrefab == null)
        {
            Debug.LogWarning("[kangtoe99_LevelUpSystem] levelUpPanel / slotContainer / slotPrefab 인스펙터 할당 필요");
            Time.timeScale = 1f;
            if (kangtoe99_GameManager.Instance != null)
                kangtoe99_GameManager.Instance.SetHudVisible(true);
            return;
        }

        for (int i = activeSlots.Count - 1; i >= 0; i--)
        {
            if (activeSlots[i] != null) Destroy(activeSlots[i].gameObject);
        }
        activeSlots.Clear();

        var chosen = BuildChoices();

        for (int i = 0; i < chosen.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Bind(chosen[i], OnChoiceSelected);
            activeSlots.Add(slot);
        }

        levelUpPanel.SetActive(true);
    }

    private List<kangtoe99_ILevelUpChoice> BuildChoices()
    {
        GameObject playerGo = player != null ? player.gameObject : null;

        // 사용 가능한 ItemData 풀
        var itemCandidates = new List<kangtoe99_ILevelUpChoice>();
        for (int i = 0; i < itemPool.Count; i++)
        {
            var item = itemPool[i];
            if (item == null) continue;
            if (((kangtoe99_ILevelUpChoice)item).IsAvailable(playerGo))
                itemCandidates.Add(item);
        }

        var result = new List<kangtoe99_ILevelUpChoice>(choiceCount);

        if (itemCandidates.Count > 0)
        {
            // ItemData 풀에 1개라도 남아있으면 ItemData만 노출 (1~choiceCount개).
            Shuffle(itemCandidates);
            int take = Mathf.Min(choiceCount, itemCandidates.Count);
            for (int i = 0; i < take; i++) result.Add(itemCandidates[i]);
        }
        else
        {
            // ItemData 풀이 완전 고갈된 경우에만 InstantDrop으로 채움.
            var dropCandidates = new List<kangtoe99_ILevelUpChoice>();
            for (int i = 0; i < instantDropPool.Count; i++)
            {
                var d = instantDropPool[i];
                if (d == null) continue;
                if (((kangtoe99_ILevelUpChoice)d).IsAvailable(playerGo))
                    dropCandidates.Add(d);
            }
            Shuffle(dropCandidates);
            int take = Mathf.Min(choiceCount, dropCandidates.Count);
            for (int i = 0; i < take; i++) result.Add(dropCandidates[i]);
        }

        return result;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void OnChoiceSelected(kangtoe99_ILevelUpChoice choice)
    {
        if (upgradeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(upgradeSound);
        }
        if (player != null) choice?.Apply(player.gameObject);
        Time.timeScale = 1f;
        if (kangtoe99_GameManager.Instance != null)
            kangtoe99_GameManager.Instance.SetHudVisible(true);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    public int GetCurrentLevel() => currentLevel;
    public int GetNextLevelScore() => nextLevelScore;

    // 디버그 패널 등 외부 도구가 풀을 조회할 때 사용. 읽기 전용.
    public IReadOnlyList<kangtoe99_ItemData> GetItemPoolSnapshot() => itemPool;
}
