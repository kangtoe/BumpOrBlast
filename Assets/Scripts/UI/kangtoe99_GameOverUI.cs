using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

public class kangtoe99_GameOverUI : MonoBehaviour
{
    public static kangtoe99_GameOverUI Instance { get; private set; }

    [Header("Leaderboard Display")]
    // 항목은 런타임에 프리팹으로 생성한다 — Top N은 topContainer, 내 주변 ±radius는 myRankContainer 에 따로 그린다.
    [SerializeField] private kangtoe99_LeaderboardEntry entryPrefab;
    [FormerlySerializedAs("entryContainer")]
    [SerializeField] private Transform topContainer;
    [SerializeField] private Transform myRankContainer;

    [Header("Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private const int topCount = 5;       // 최상위 N
    private const int aroundRadius = 2;   // 내 순위 기준 위·아래로 N칸

    // State
    private int myRankIndex = -1;
    private int myRankId = -1;
    private int currentScore;
    private int currentLevel;
    private RankData[] allRanksData;

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
        // 전체 랭킹 조회 — 내 순위는 결과에서 myRankId 로 직접 찾는다 (별도 GetMyRank 호출의 race 회피).
        RankData[] allRanks = null;
        bool done = false;
        StartCoroutine(kangtoe99_RankApi.Instance.GetAllRanks(
            onSuccess: (ranks) =>
            {
                allRanks = ranks;
                done = true;
            },
            onError: (error) =>
            {
                Debug.LogWarning($"전체 랭킹 조회 실패: {error}");
                done = true;
            }
        ));

        yield return new WaitUntil(() => done);

        allRanksData = allRanks;
        myRankIndex = FindMyRankIndex(allRanks, myRankId);
        Debug.Log($"My rank index: {myRankIndex} (id={myRankId}, total={allRanks?.Length ?? 0})");

        RenderLeaderboard();
    }

    // Top N 은 topContainer, 내 주변 ±radius 는 myRankContainer 에 각각 그린다. 두 영역 사이 중복은 그대로 둔다.
    private void RenderLeaderboard()
    {
        if (entryPrefab == null) return;

        ClearContainer(topContainer);
        ClearContainer(myRankContainer);

        if (allRanksData == null) return;

        // Top N
        if (topContainer != null)
        {
            for (int i = 0; i < topCount && i < allRanksData.Length; i++)
            {
                CreateEntry(topContainer, i);
            }
        }

        // 내 주변 ±radius
        if (myRankContainer != null && myRankIndex >= 0)
        {
            for (int i = myRankIndex - aroundRadius; i <= myRankIndex + aroundRadius; i++)
            {
                if (i >= 0 && i < allRanksData.Length)
                {
                    CreateEntry(myRankContainer, i);
                }
            }
        }
    }

    private static int FindMyRankIndex(RankData[] ranks, int id)
    {
        if (ranks == null || id <= 0) return -1;
        for (int i = 0; i < ranks.Length; i++)
        {
            if (ranks[i].id == id) return i;
        }
        return -1;
    }

    private static void ClearContainer(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private void CreateEntry(Transform container, int idx)
    {
        RankData data = allRanksData[idx];
        kangtoe99_LeaderboardEntry entry = Instantiate(entryPrefab, container, false);

        // 내 항목 이름은 항상 로컬 PlayerName으로 표시 — 서버 저장/PATCH 타이밍과 무관하게 일관되게.
        bool isMine = data.id == myRankId;
        string displayName = isMine && kangtoe99_GameManager.Instance != null
            ? kangtoe99_GameManager.Instance.PlayerName
            : data.name;

        entry.SetData(idx + 1, displayName, data.level, data.score);
        entry.SetColor(isMine ? highlightColor : normalColor);
    }

    // 종합 정보 패널의 이름 편집에서 호출 — 화면을 즉시 갱신하고 서버 항목 이름도 갱신한다.
    public void UpdateMyRankName(string newName)
    {
        // 내 항목 이름은 RenderLeaderboard가 GameManager.PlayerName에서 읽으므로 재렌더만 하면 반영된다.
        RenderLeaderboard();

        // 서버 항목 이름 갱신 (best-effort)
        if (myRankId > 0 && kangtoe99_RankApi.Instance != null)
        {
            StartCoroutine(kangtoe99_RankApi.Instance.UpdateRankName(
                myRankId,
                newName,
                onSuccess: (rankData) => Debug.Log($"Rank name updated on server: {rankData.name}"),
                onError: (error) => Debug.LogWarning($"랭킹 이름 서버 갱신 실패: {error}")
            ));
        }
    }

    #endregion
}
