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
    [Tooltip("레벨 1→2 에 필요한 점수.")]
    [SerializeField] private int baseScore = 100;
    [Tooltip("레벨당 요구 점수 증가 배수 (지수형, BB2와 동일). 1.5 면 100 → 150 → 225 → 338 ...")]
    [SerializeField, Min(1f)] private float growth = 1.5f;
    [SerializeField, Min(1)] private int choiceCount = 4;

    [Header("Player")]
    [SerializeField] private kangtoe99_Player player;

    [Header("Choice Pool")]
    // Assets/Resources/Items/ 하위의 모든 ItemData를 Awake에서 자동 수집. 인스펙터 할당 불필요.
    // 새 아이템은 Resources/Items/Tier{N}_*/ 폴더에 자산만 드롭하면 자동 포함된다.
    private readonly List<kangtoe99_ItemData> itemPool = new List<kangtoe99_ItemData>();
    [Tooltip("아이템 풀이 부족할 때 슬롯을 채우는 폴백. 현재 구현 드롭 3종(XP/HP/Bomb) 권장.")]
    [SerializeField] private List<kangtoe99_InstantDropItemData> instantDropPool = new List<kangtoe99_InstantDropItemData>();
    [Tooltip("티어별 등장 가중치(레벨 비례). 미할당이면 균등 추첨으로 폴백.")]
    [SerializeField] private kangtoe99_TierDropTable tierDropTable;

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
    [SerializeField] private AudioClip rerollSound;

    [Header("Level Up FX")]
    [SerializeField] private string levelUpFxText = "LEVEL UP!";
    [SerializeField] private Color levelUpFxColor = Color.white;
    [SerializeField] private float levelUpFxBurstRadius = 3f;
    [SerializeField] private float levelUpFxBurstKnockback = 12f;

    private int currentLevel = 0;
    private int nextLevelScore;
    private int previousLevelScore = 0;
    private int pendingLevelUps = 0;
    private int rerollPoints;
    private bool panelOpen = false;
    private AudioSource audioSource;
    private readonly List<kangtoe99_LevelUpChoiceSlot> activeSlots = new List<kangtoe99_LevelUpChoiceSlot>();
    // 현재 패널에 표시 중인 선택지 캐시. 리롤 / 선택 시에만 새로 추첨하고, 단순 재오픈(ESC 후 다시 열기) 시에는 유지한다.
    private List<kangtoe99_ILevelUpChoice> currentChoices = new List<kangtoe99_ILevelUpChoice>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        nextLevelScore = baseScore;
        rerollPoints = startingRerollPoints;

        LoadItemPool();

        if (player == null) player = FindFirstObjectByType<kangtoe99_Player>();

        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        if (rerollButton != null) rerollButton.onClick.AddListener(Reroll);

        UpdatePendingUI();
        UpdateRerollUI();
        UpdateHudPrompt();
    }

    // 티어 폴더 경로 — 인덱스 = (int)kangtoe99_Tier.
    // 새 티어 추가 시 이 배열과 kangtoe99_Tier enum, kangtoe99_TierStackLimits 함께 갱신.
    private static readonly string[] TierFolderPaths = {
        "Items/Tier0_Gray",
        "Items/Tier1_Green",
        "Items/Tier2_Blue",
        "Items/Tier3_Purple",
        "Items/Tier4_Orange",
    };

    // 티어 폴더별 ItemData 자산을 로드하고 ItemTierRegistry에 등록.
    // 새 아이템 추가 = 해당 티어 폴더에 .asset 드롭만 하면 끝. tier 필드 따로 설정 불필요.
    private void LoadItemPool()
    {
        itemPool.Clear();
        kangtoe99_ItemTierRegistry.Clear();
        for (int t = 0; t < TierFolderPaths.Length; t++)
        {
            var tier = (kangtoe99_Tier)t;
            var loaded = Resources.LoadAll<kangtoe99_ItemData>(TierFolderPaths[t]);
            for (int i = 0; i < loaded.Length; i++)
            {
                if (loaded[i] == null) continue;
                kangtoe99_ItemTierRegistry.Register(loaded[i], tier);
                itemPool.Add(loaded[i]);
            }
        }
        Debug.Log($"[LevelUpSystem] itemPool 로드 — {itemPool.Count}개 (Resources/Items, 티어 폴더별)");
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
        // BB2 PlayerLevel.ComputeNeeded 와 동일한 지수형 곡선.
        int requiredScore = Mathf.Max(1, Mathf.CeilToInt(baseScore * Mathf.Pow(growth, currentLevel - 1)));
        nextLevelScore = previousLevelScore + requiredScore;
        pendingLevelUps++;

        if (levelUpSound != null && audioSource != null)
            audioSource.PlayOneShot(levelUpSound);

        EmitLevelUpFx();

        UpdatePendingUI();
        UpdateHudPrompt();
    }

    private void EmitLevelUpFx()
    {
        if (player == null) return;
        Vector2 pos = player.transform.position;

        if (kangtoe99_FloatingTextManager.Instance != null)
            kangtoe99_FloatingTextManager.Instance.ShowAtPlayer(levelUpFxText, levelUpFxColor);

        if (kangtoe99_ExplosionManager.Instance != null)
            kangtoe99_ExplosionManager.Instance.SpawnOne(pos, 0f, levelUpFxBurstRadius, levelUpFxBurstKnockback, levelUpFxColor);
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

        // 캐시된 선택지가 있으면 그대로 유지(단순 재오픈), 없을 때만 새로 추첨한다.
        if (currentChoices == null || currentChoices.Count == 0)
            RegenerateChoices();
        DisplayChoices();
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

    // 선택지를 새로 추첨해 캐시에 저장. 리롤 / 선택 후에만 호출 — 단순 재오픈 시에는 호출하지 않아 선택지가 유지된다.
    private void RegenerateChoices()
    {
        currentChoices = BuildChoices();
    }

    // 캐시된 선택지(currentChoices)로 슬롯을 다시 빌드하고 패널을 표시.
    private void DisplayChoices()
    {
        for (int i = activeSlots.Count - 1; i >= 0; i--)
        {
            if (activeSlots[i] != null) Destroy(activeSlots[i].gameObject);
        }
        activeSlots.Clear();

        for (int i = 0; i < currentChoices.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Bind(currentChoices[i], OnChoiceSelected);
            activeSlots.Add(slot);
        }

        levelUpPanel.SetActive(true);
    }

    public void Reroll()
    {
        if (!panelOpen || rerollPoints <= 0) return;
        rerollPoints--;
        if (rerollSound != null && audioSource != null)
            audioSource.PlayOneShot(rerollSound);
        UpdateRerollUI();
        RegenerateChoices();
        DisplayChoices();
    }

    private List<kangtoe99_ILevelUpChoice> BuildChoices()
    {
        GameObject playerGo = player != null ? player.gameObject : null;

        // 사용 가능한 ItemData 풀 (티어 가중 추첨을 위해 ItemData로 타입 보유)
        var itemCandidates = new List<kangtoe99_ItemData>();
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
            // 티어 가중 독립 추첨. 같은 아이템은 중복 금지(뽑힌 항목은 후보에서 제거).
            int take = Mathf.Min(choiceCount, itemCandidates.Count);
            for (int i = 0; i < take; i++)
            {
                int pickedIdx = PickWeightedIndex(itemCandidates);
                if (pickedIdx < 0) break;
                result.Add(itemCandidates[pickedIdx]);
                itemCandidates.RemoveAt(pickedIdx);
            }
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

    // 티어 가중치 기반 인덱스 추첨. tierDropTable 미할당 또는 합계 0이면 균등 폴백.
    private int PickWeightedIndex(List<kangtoe99_ItemData> candidates)
    {
        if (candidates.Count == 0) return -1;
        if (tierDropTable == null)
            return Random.Range(0, candidates.Count);

        int level = Mathf.Max(1, currentLevel);
        float total = 0f;
        for (int i = 0; i < candidates.Count; i++)
            total += tierDropTable.Evaluate(candidates[i].Tier, level);

        if (total <= 0f)
            return Random.Range(0, candidates.Count);

        float r = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            acc += tierDropTable.Evaluate(candidates[i].Tier, level);
            if (r <= acc) return i;
        }
        return candidates.Count - 1;
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
            RegenerateChoices();
            DisplayChoices();
            UpdateRerollUI();
            UpdateHudPrompt();
            return;
        }
        // 모든 pending 소진 — 캐시를 비워 다음 오픈 시 새 선택지를 추첨한다.
        currentChoices.Clear();
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
