using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kangtoe99_GameOverUI : MonoBehaviour
{
    public static kangtoe99_GameOverUI Instance { get; private set; }

    [Header("Leaderboard Display")]
    // 항목은 런타임에 프리팹으로 생성한다 — 최상위 5 + 내 주변 5(중복 허용)를 한 명단으로 표시.
    [SerializeField] private kangtoe99_LeaderboardEntry entryPrefab;
    [SerializeField] private Transform entryContainer;

    [Header("Build Display")]
    [SerializeField] private kangtoe99_BuildDisplayUI buildDisplay;

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

        // 빌드 영역 갱신 (패널이 켜질 때 OnEnable에서도 호출되지만 명시적으로 한 번 더)
        if (buildDisplay != null) buildDisplay.Refresh();

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
        allRanksData = allRanks;
        RenderLeaderboard();
    }

    // 최상위 5 + 내 주변 5(중복 허용)를 한 명단으로 그린다.
    private void RenderLeaderboard()
    {
        if (entryContainer == null || entryPrefab == null) return;

        // 기존 항목 제거
        for (int i = entryContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(entryContainer.GetChild(i).gameObject);
        }

        if (allRanksData == null) return;

        // 표시할 인덱스 목록 구성: 최상위 N → 내 주변(±radius). 겹쳐도 중복 그대로 둔다.
        List<int> indices = new List<int>();

        for (int i = 0; i < topCount && i < allRanksData.Length; i++)
        {
            indices.Add(i);
        }

        if (myRankIndex >= 0)
        {
            for (int i = myRankIndex - aroundRadius; i <= myRankIndex + aroundRadius; i++)
            {
                if (i >= 0 && i < allRanksData.Length)
                {
                    indices.Add(i);
                }
            }
        }

        // 항목 생성
        foreach (int idx in indices)
        {
            RankData data = allRanksData[idx];
            kangtoe99_LeaderboardEntry entry = Instantiate(entryPrefab, entryContainer, false);

            // 내 항목 이름은 항상 로컬 PlayerName으로 표시 — 서버 저장/PATCH 타이밍과 무관하게 일관되게.
            bool isMine = data.id == myRankId;
            string displayName = isMine && kangtoe99_GameManager.Instance != null
                ? kangtoe99_GameManager.Instance.PlayerName
                : data.name;

            entry.SetData(idx + 1, displayName, data.level, data.score);
            entry.SetColor(isMine ? highlightColor : normalColor);
        }
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
