using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LeaderboardEntry
{
    public int score;
    public int level;

    public LeaderboardEntry(int score, int level)
    {
        this.score = score;
        this.level = level;
    }
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class kangtoe99_GameOverUI : MonoBehaviour
{
    public static kangtoe99_GameOverUI Instance { get; private set; }

    [Header("Leaderboard Display")]
    [SerializeField] private kangtoe99_LeaderboardEntry[] rankEntries;

    [Header("My Rank Display")]
    [SerializeField] private kangtoe99_LeaderboardEntry myRankEntry;

    [Header("Settings")]
    [SerializeField] private int maxLeaderboardEntries = 10;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    // Leaderboard Data
    private const string LEADERBOARD_KEY = "Leaderboard";
    private LeaderboardData leaderboardData;

    // State
    private int myRankIndex = -1;
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

        LoadLeaderboard();
    }

    #region Leaderboard Data Management

    private void LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            leaderboardData = JsonUtility.FromJson<LeaderboardData>(json);
        }
        else
        {
            leaderboardData = new LeaderboardData();
        }
    }

    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(leaderboardData);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();
    }

    private int AddLeaderboardEntry(int score, int level)
    {
        LeaderboardEntry newEntry = new LeaderboardEntry(score, level);

        // 같은 점수보다 뒤에 삽입 (나중에 등록된 기록이 낮은 순위)
        int insertIndex = 0;
        for (int i = 0; i < leaderboardData.entries.Count; i++)
        {
            if (score <= leaderboardData.entries[i].score)
            {
                insertIndex = i + 1;
            }
            else
            {
                break;
            }
        }
        leaderboardData.entries.Insert(insertIndex, newEntry);

        // 최대 엔트리 수 제한
        if (leaderboardData.entries.Count > maxLeaderboardEntries)
        {
            leaderboardData.entries.RemoveAt(leaderboardData.entries.Count - 1);
        }

        SaveLeaderboard();

        return insertIndex;
    }

    #endregion

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

        myRankIndex = AddLeaderboardEntry(score, currentLevel);
        ShowLeaderboard();
    }

    private void ShowLeaderboard()
    {
        if (rankEntries != null)
        {
            List<LeaderboardEntry> entries = leaderboardData.entries;

            // 고정 엔트리에 데이터 설정
            for (int i = 0; i < rankEntries.Length; i++)
            {
                if (rankEntries[i] == null) continue;

                if (i < entries.Count)
                {
                    rankEntries[i].SetData(i + 1, entries[i].level, entries[i].score);
                    rankEntries[i].SetColor(i == myRankIndex ? highlightColor : normalColor);
                }
                else
                {
                    rankEntries[i].SetEmpty(i + 1);
                    rankEntries[i].SetColor(normalColor);
                }
            }

            // 내 순위 항상 표시
            if (myRankEntry != null)
            {
                myRankEntry.SetData(myRankIndex + 1, currentLevel, currentScore);
                myRankEntry.SetColor(highlightColor);
            }
        }
    }

    #endregion
}
