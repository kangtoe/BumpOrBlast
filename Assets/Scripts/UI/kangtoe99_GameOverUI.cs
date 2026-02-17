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

    [Header("Player Name")]
    [SerializeField] private InputField nameInput;
    [SerializeField] private string defaultPlayerName = "Player";

    [Header("Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    // State
    private int myRankIndex = -1;
    private int myRankId = -1;
    private int currentScore;
    private int currentLevel;

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

        string playerName = (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
            ? nameInput.text
            : defaultPlayerName;

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
        if (rankEntries == null) return;

        for (int i = 0; i < rankEntries.Length; i++)
        {
            if (rankEntries[i] == null) continue;

            if (allRanks != null && i < allRanks.Length)
            {
                rankEntries[i].SetData(i + 1, allRanks[i].level, allRanks[i].score);
                rankEntries[i].SetColor(allRanks[i].id == myRankId ? highlightColor : normalColor);
            }
            else
            {
                rankEntries[i].SetEmpty(i + 1);
                rankEntries[i].SetColor(normalColor);
            }
        }

        // 내 순위 항상 표시
        if (myRankEntry != null && myRankIndex >= 0)
        {
            myRankEntry.SetData(myRankIndex + 1, currentLevel, currentScore);
            myRankEntry.SetColor(highlightColor);
        }
    }

    #endregion
}
