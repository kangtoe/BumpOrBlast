using UnityEngine;
using TMPro;

public class kangtoe99_ScoreSystem : MonoBehaviour
{
    public static kangtoe99_ScoreSystem Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    private int highScore = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 최고 점수 불러오기
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int score)
    {
        currentScore += score;
        UpdateScoreUI();

        // 최고 점수 갱신
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentScore}";
        }
    }

    public int GetCurrentScore() => currentScore;
    public int GetHighScore() => highScore;
}
