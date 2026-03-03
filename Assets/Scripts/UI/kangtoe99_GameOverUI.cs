using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class kangtoe99_GameOverUI : MonoBehaviour
{
    public static kangtoe99_GameOverUI Instance { get; private set; }

    [Header("Leaderboard Display")]
    [SerializeField] private kangtoe99_LeaderboardEntry[] rankEntries;

    [Header("My Rank Display")]
    [SerializeField] private kangtoe99_LeaderboardEntry myRankEntry;

    [Header("Pagination")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Text pageText;

    [Header("Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private const int entriesPerPage = 5;

    // State
    private int myRankIndex = -1;
    private int myRankId = -1;
    private int currentScore;
    private int currentLevel;
    private RankData[] allRanksData;
    private int currentPage = 0;

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
    }

    private void Start()
    {
        if (prevButton != null)
            prevButton.onClick.AddListener(OnPrevPage);
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextPage);
    }

    #region UI Display

    public void ShowGameOver(int score)
    {
        currentScore = score;
        currentLevel = kangtoe99_LevelUpSystem.Instance != null
            ? kangtoe99_LevelUpSystem.Instance.GetCurrentLevel()
            : 1;

        // 크로스헤어 비활성화
        if (kangtoe99_CrosshairUI.Instance != null)
        {
            kangtoe99_CrosshairUI.Instance.gameObject.SetActive(false);
        }

        // 서버에 랭킹 저장
        SubmitRankToServer();
    }

    private void SubmitRankToServer()
    {
        if (kangtoe99_RankApi.Instance == null) return;

        string playerName = kangtoe99_GameManager.Instance != null
            ? kangtoe99_GameManager.Instance.PlayerName
            : "Player";

        StartCoroutine(kangtoe99_RankApi.Instance.CreateRank(
            currentLevel,
            playerName,
            currentScore,
            onSuccess: (rankData) =>
            {
                Debug.Log($"Rank created - id: {rankData.id}, name: {rankData.name}, level: {rankData.level}, score: {rankData.score}");
                myRankId = rankData.id;

                // 내 순위 조회 → 전체 랭킹 조회
                StartCoroutine(FetchMyRankAndLeaderboard());
            },
            onError: (error) =>
            {
                Debug.LogWarning($"랭킹 서버 저장 실패: {error}");
            }
        ));
    }

    private IEnumerator FetchMyRankAndLeaderboard()
    {
        // 1. 내 순위 조회
        bool myRankDone = false;
        StartCoroutine(kangtoe99_RankApi.Instance.GetMyRank(
            myRankId,
            onSuccess: (myRank) =>
            {
                Debug.Log($"My rank: {myRank.rank} / {myRank.total}");
                myRankIndex = myRank.rank - 1;
                myRankDone = true;
            },
            onError: (error) =>
            {
                Debug.LogWarning($"내 순위 조회 실패: {error}");
                myRankDone = true;
            }
        ));

        // 2. 전체 랭킹 조회
        RankData[] allRanks = null;
        bool allRanksDone = false;
        StartCoroutine(kangtoe99_RankApi.Instance.GetAllRanks(
            onSuccess: (ranks) =>
            {
                allRanks = ranks;
                allRanksDone = true;
            },
            onError: (error) =>
            {
                Debug.LogWarning($"전체 랭킹 조회 실패: {error}");
                allRanksDone = true;
            }
        ));

        // 두 요청 모두 완료될 때까지 대기
        yield return new WaitUntil(() => myRankDone && allRanksDone);

        // 리더보드 UI 갱신
        ShowLeaderboard(allRanks);
    }

    private void ShowLeaderboard(RankData[] allRanks)
    {
        Debug.Log($"ShowLeaderboard - rankEntries: {rankEntries?.Length}, allRanks: {allRanks?.Length}");
        if (rankEntries == null) return;

        allRanksData = allRanks;
        currentPage = 0;
        ShowPage(currentPage);

        // 내 순위 항상 표시
        if (myRankEntry != null && myRankIndex >= 0)
        {
            string playerName = kangtoe99_GameManager.Instance != null
                ? kangtoe99_GameManager.Instance.PlayerName
                : "Player";
            myRankEntry.SetData(myRankIndex + 1, playerName, currentLevel, currentScore);
            myRankEntry.SetColor(highlightColor);
        }
    }

    private void ShowPage(int page)
    {
        int startIndex = page * entriesPerPage;

        for (int i = 0; i < rankEntries.Length; i++)
        {
            if (rankEntries[i] == null) continue;

            int dataIndex = startIndex + i;
            if (allRanksData != null && dataIndex < allRanksData.Length)
            {
                rankEntries[i].SetData(dataIndex + 1, allRanksData[dataIndex].name, allRanksData[dataIndex].level, allRanksData[dataIndex].score);
                rankEntries[i].SetColor(allRanksData[dataIndex].id == myRankId ? highlightColor : normalColor);
            }
            else
            {
                rankEntries[i].SetEmpty(startIndex + i + 1);
                rankEntries[i].SetColor(normalColor);
            }
        }

        // 페이지네이션 UI 업데이트
        int totalPages = allRanksData != null
            ? Mathf.Max(1, Mathf.CeilToInt((float)allRanksData.Length / entriesPerPage))
            : 1;

        if (pageText != null)
            pageText.text = $"{page + 1} / {totalPages}";

        if (prevButton != null)
            prevButton.interactable = page > 0;

        if (nextButton != null)
            nextButton.interactable = page < totalPages - 1;
    }

    public void OnPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    public void OnNextPage()
    {
        int totalPages = allRanksData != null
            ? Mathf.Max(1, Mathf.CeilToInt((float)allRanksData.Length / entriesPerPage))
            : 1;

        if (currentPage < totalPages - 1)
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    #endregion
}
