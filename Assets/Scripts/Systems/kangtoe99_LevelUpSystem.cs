using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class kangtoe99_LevelUpSystem : MonoBehaviour
{
    public static kangtoe99_LevelUpSystem Instance { get; private set; }

    // LevelUpSystem 이 ESC 를 패널 닫기에 소비한 프레임. PauseSystem 이 같은 프레임 ESC 를 무시할 때 사용.
    // (스크립트 실행 순서에 따라 PauseSystem 이 늦게 돌면 ClosePanel 직후 timeScale=1 이 되어
    // CanToggleNow 통과 → 의도치 않은 Pause 진입을 막기 위함.)
    public static int LastEscapeConsumedFrame { get; private set; } = -1;

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
    [SerializeField] private TMP_Text pendingLevelUpsText; // 패널 내 "Pending Level-Ups: N"
    [SerializeField] private TMP_Text hudPromptText; // 인게임 "Level Up Available! Press <Key> (xN)"

    [Header("Pending / Input")]
    [Tooltip("점수가 임계치를 넘으면 즉시 패널을 띄우지 않고 카운트만 누적, 이 키로 오픈한다.")]
    [SerializeField] private KeyCode openPanelKey = KeyCode.Space;

    [Header("Reroll")]
    [SerializeField, Min(0)] private int startingRerollPoints = 10;
    [SerializeField] private TMP_Text rerollPointsText;
    [SerializeField] private Button rerollButton;

    [Header("SFX")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip upgradeSound;

    private int currentLevel = 0;
    private int nextLevelScore;
    private int previousLevelScore = 0;
    private int pendingLevelUps = 0;
    private int rerollPoints;
    private bool panelOpen = false;
    private AudioSource audioSource;
    private readonly List<kangtoe99_LevelUpChoiceSlot> activeSlots = new List<kangtoe99_LevelUpChoiceSlot>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        nextLevelScore = baseScore;
        rerollPoints = startingRerollPoints;

        if (player == null) player = FindFirstObjectByType<kangtoe99_Player>();

        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (rerollButton != null) rerollButton.onClick.AddListener(Reroll);

        UpdatePendingUI();
        UpdateRerollUI();
        UpdateHudPrompt();
    }

    private void Update()
    {
        CheckLevelUp();
        UpdateExpBar();

        // 패널이 닫혀있고 적립된 레벨업이 있으면 키 입력으로 오픈.
        // Time.timeScale 영향 없는 Input.GetKeyDown 사용.
        if (!panelOpen && pendingLevelUps > 0 && Input.GetKeyDown(openPanelKey))
        {
            OpenPanel();
        }
        // 패널이 열려있을 때 ESC 로 닫음 — pending 은 유지되어 HUD 프롬프트가 다시 표시된다.
        // LastEscapeConsumedFrame 기록으로 PauseSystem 이 같은 프레임 늦게 돌더라도 Pause 진입 방지.
        else if (panelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            LastEscapeConsumedFrame = Time.frameCount;
            ClosePanel();
        }
    }

    private void UpdateExpBar()
    {
        if (expBar == null || kangtoe99_ScoreSystem.Instance == null) return;
        int currentScore = kangtoe99_ScoreSystem.Instance.GetCurrentScore();
        float progress = (float)(currentScore - previousLevelScore) / (nextLevelScore - previousLevelScore);
        expBar.fillAmount = Mathf.Clamp01(progress);
    }

    private void UpdatePendingUI()
    {
        if (pendingLevelUpsText != null)
            pendingLevelUpsText.text = $"Left Points: {pendingLevelUps}";
    }

    private void UpdateRerollUI()
    {
        if (rerollPointsText != null)
            rerollPointsText.text = $"Rerolls: {rerollPoints}";
        if (rerollButton != null)
            rerollButton.interactable = panelOpen && rerollPoints > 0;
    }

    private void UpdateHudPrompt()
    {
        if (hudPromptText == null) return;
        bool show = pendingLevelUps > 0 && !panelOpen;
        hudPromptText.gameObject.SetActive(show);
        if (show)
            hudPromptText.text = $"Press [{openPanelKey}] to Upgrade! +{pendingLevelUps}";
    }

    private void CheckLevelUp()
    {
        if (kangtoe99_ScoreSystem.Instance == null) return;
        int currentScore = kangtoe99_ScoreSystem.Instance.GetCurrentScore();
        if (currentScore >= nextLevelScore) LevelUp();
    }

    // 점수 임계치 도달 — 즉시 패널을 띄우지 않고 카운트만 누적.
    // 플레이어가 openPanelKey 로 직접 열어야 선택지가 표시된다.
    private void LevelUp()
    {
        currentLevel++;
        previousLevelScore = nextLevelScore;
        int requiredScore = baseScore + (currentLevel - 1) * scorePerLevel;
        nextLevelScore = previousLevelScore + requiredScore;
        pendingLevelUps++;

        if (levelUpSound != null && audioSource != null)
            audioSource.PlayOneShot(levelUpSound);

        UpdatePendingUI();
        UpdateHudPrompt();
    }

    private void OpenPanel()
    {
        if (panelOpen) return;
        if (pendingLevelUps <= 0) return;
        if (levelUpPanel == null || slotContainer == null || slotPrefab == null)
        {
            Debug.LogWarning("[kangtoe99_LevelUpSystem] levelUpPanel / slotContainer / slotPrefab 인스펙터 할당 필요");
            return;
        }

        Time.timeScale = 0f;
        if (kangtoe99_GameManager.Instance != null)
            kangtoe99_GameManager.Instance.SetHudVisible(false);
        panelOpen = true;

        ShowChoices();
        UpdateRerollUI();
        UpdateHudPrompt();
    }

    private void ClosePanel()
    {
        panelOpen = false;
        Time.timeScale = 1f;
        if (kangtoe99_GameManager.Instance != null)
            kangtoe99_GameManager.Instance.SetHudVisible(true);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        UpdateRerollUI();
        UpdateHudPrompt();
    }

    // 패널에 4개 슬롯을 새로 빌드. OpenPanel / OnChoiceSelected(잔여 pending) / Reroll 에서 호출.
    private void ShowChoices()
    {
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

    public void Reroll()
    {
        if (!panelOpen || rerollPoints <= 0) return;
        rerollPoints--;
        UpdateRerollUI();
        ShowChoices();
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
            audioSource.PlayOneShot(upgradeSound);
        if (player != null) choice?.Apply(player.gameObject);

        pendingLevelUps--;
        UpdatePendingUI();

        // 남은 pending 이 있으면 패널 유지 + 새 선택지로 자동 재오픈.
        if (pendingLevelUps > 0)
        {
            ShowChoices();
            UpdateRerollUI();
            UpdateHudPrompt();
            return;
        }
        ClosePanel();
    }

    public int GetCurrentLevel() => currentLevel;
    public int GetNextLevelScore() => nextLevelScore;
    public int GetPendingLevelUps() => pendingLevelUps;
    public int GetRerollPoints() => rerollPoints;

    // 디버그 패널 등 외부 도구가 풀을 조회할 때 사용. 읽기 전용.
    public IReadOnlyList<kangtoe99_ItemData> GetItemPoolSnapshot() => itemPool;

    // 디버그 패널용 — 즉시 패널 오픈 / pending 강제 추가 / 리롤 포인트 충전.
    public void DebugForceOpenPanel()
    {
        if (pendingLevelUps <= 0) pendingLevelUps = 1;
        UpdatePendingUI();
        OpenPanel();
    }

    public void DebugAddPendingLevelUp(int amount = 1)
    {
        pendingLevelUps += Mathf.Max(1, amount);
        UpdatePendingUI();
        UpdateHudPrompt();
    }

    public void DebugAddRerollPoints(int amount = 5)
    {
        rerollPoints += Mathf.Max(1, amount);
        UpdateRerollUI();
    }
}
