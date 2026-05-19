using UnityEngine;
using UnityEngine.Serialization;

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

        // 임시 로컬 랭킹 저장 — 서버 /rank/around 엔드포인트 추가 후 RankApi 경로로 복귀 예정.
        SubmitRankLocal();
    }

    private void SubmitRankLocal()
    {
        string playerName = kangtoe99_GameManager.Instance != null
            ? kangtoe99_GameManager.Instance.PlayerName
            : "Player";

        var rank = kangtoe99_LocalRankStore.Create(currentLevel, playerName, currentScore);
        myRankId = rank.id;
        Debug.Log($"Rank created (local) - id: {rank.id}, name: {rank.name}, level: {rank.level}, score: {rank.score}");

        allRanksData = kangtoe99_LocalRankStore.GetAllSorted();
        myRankIndex = FindMyRankIndex(allRanksData, myRankId);
        Debug.Log($"My rank index: {myRankIndex} (id={myRankId}, total={allRanksData?.Length ?? 0})");

        RenderLeaderboard();
    }

    // Top N 은 topContainer, 내 주변 ±radius 는 myRankContainer 에 각각 그린다. 두 영역 사이 중복은 그대로 둔다.
    // 데이터가 없는 슬롯은 '-' 로 채워 슬롯 개수는 항상 일정하게 유지.
    private void RenderLeaderboard()
    {
        if (entryPrefab == null) return;

        ClearContainer(topContainer);
        ClearContainer(myRankContainer);

        int total = allRanksData != null ? allRanksData.Length : 0;

        // Top N — 항상 topCount 슬롯 채움.
        if (topContainer != null)
        {
            for (int i = 0; i < topCount; i++)
            {
                if (i < total) CreateEntry(topContainer, i);
                else CreateEmptyEntry(topContainer, i + 1);
            }
        }

        // 내 주변 ±radius — 항상 2*radius+1 슬롯. 내 순위 미확보면 전부 '-'.
        if (myRankContainer != null)
        {
            int slots = 2 * aroundRadius + 1;
            if (myRankIndex >= 0)
            {
                for (int offset = -aroundRadius; offset <= aroundRadius; offset++)
                {
                    int idx = myRankIndex + offset;
                    if (idx >= 0 && idx < total) CreateEntry(myRankContainer, idx);
                    else CreateEmptyEntry(myRankContainer, idx + 1); // idx+1 ≤ 0 이면 SetEmpty 가 '-' 처리
                }
            }
            else
            {
                for (int i = 0; i < slots; i++) CreateEmptyEntry(myRankContainer, 0);
            }
        }
    }

    private void CreateEmptyEntry(Transform container, int rank)
    {
        kangtoe99_LeaderboardEntry entry = Instantiate(entryPrefab, container, false);
        entry.SetEmpty(rank);
        entry.SetColor(normalColor);
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
