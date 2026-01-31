using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class kangtoe99_GameManager : MonoBehaviour
{
    public static kangtoe99_GameManager Instance { get; private set; }

    [Header("Game State")]
    private bool isGameStarted = false;
    private bool isGameOver = false;

    [Header("UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Text startText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;

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

        // 게임 시작 전 상태
        isGameStarted = false;

        // 시작 패널 표시
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (startText != null)
        {
            startText.text = "Press [Any Key] to Start!";
        }

        // 게임오버 패널 숨기기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // 게임 시작 대기 중
        if (!isGameStarted && Input.anyKeyDown)
        {
            StartGame();
        }

        // 게임 오버 상태에서 R키로 재시작
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void StartGame()
    {
        isGameStarted = true;

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        // 적 스포너 시작
        if (kangtoe99_EnemySpawner.Instance != null)
        {
            kangtoe99_EnemySpawner.Instance.StartSpawning();
        }

        Debug.Log("Game Started!");
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        // 적 스포너 중지
        if (kangtoe99_EnemySpawner.Instance != null)
        {
            kangtoe99_EnemySpawner.Instance.StopSpawning();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            int finalScore = kangtoe99_ScoreSystem.Instance != null
                ? kangtoe99_ScoreSystem.Instance.GetCurrentScore()
                : 0;
            int highScore = kangtoe99_ScoreSystem.Instance != null
                ? kangtoe99_ScoreSystem.Instance.GetHighScore()
                : 0;

            gameOverText.text = $"Game Over\n\nScore: {finalScore}\nHigh Score: {highScore}\n\nPress R to Restart";
        }

        Debug.Log("Game Over!");
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isGameStarted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameStarted() => isGameStarted;
    public bool IsGameOver() => isGameOver;
}
