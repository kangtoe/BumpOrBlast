using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class kangtoe99_GameManager : MonoBehaviour
{
    public static kangtoe99_GameManager Instance { get; private set; }

    [Header("Game State")]
    private bool isGameStarted = false;
    private bool isGameOver = false;

    [Header("UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private kangtoe99_GameOverUI gameOverUI;

    [Header("Player Name")]
    [SerializeField] private InputField nameInput;
    public string PlayerName { get; private set; }

    [Header("Game Over Settings")]
    [SerializeField] private float slowMotionScale = 0.2f;
    [SerializeField] private float timeRecoveryDuration = 1.5f;

    [Header("Help")]
    [SerializeField] private GameObject helpObject;
    [SerializeField] private KeyCode helpToggleKey = KeyCode.H;
    [SerializeField] private AudioClip helpOnSound;
    [SerializeField] private AudioClip helpOffSound;

    [Header("SFX")]
    [SerializeField] private AudioClip sceneStartSound;
    [SerializeField] private AudioClip gameStartSound;
    [SerializeField] private AudioClip gameOverSound;

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

        // 기본 이름 생성 (플레이스홀더에 표기)
        PlayerName = "user" + Random.Range(1000, 10000);
        if (nameInput != null)
        {
            nameInput.text = "";
            if (nameInput.placeholder != null)
            {
                Text placeholderText = nameInput.placeholder.GetComponent<Text>();
                if (placeholderText != null)
                    placeholderText.text = PlayerName;
            }
        }

        // 시작 패널 표시
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        // 게임오버 패널 숨기기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // 씬 시작 사운드 재생
        if (sceneStartSound != null)
        {
            AudioSource.PlayClipAtPoint(sceneStartSound, Camera.main.transform.position);
        }
    }

    private void Update()
    {
        // 게임 시작 대기 중 (Enter 키로만 시작)
        if (!isGameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }

        // 게임 오버 상태에서 R키로 재시작
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        // 도움말 토글
        if (Input.GetKeyDown(helpToggleKey))
        {
            ToggleHelp();
        }
    }

    private void StartGame()
    {
        isGameStarted = true;

        // 입력된 이름 저장
        if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
        {
            PlayerName = nameInput.text;
        }

        // 게임 시작 사운드 재생
        if (gameStartSound != null)
        {
            AudioSource.PlayClipAtPoint(gameStartSound, Camera.main.transform.position);
        }

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

        // 적 스포너 중지
        if (kangtoe99_EnemySpawner.Instance != null)
        {
            kangtoe99_EnemySpawner.Instance.StopSpawning();
        }

        // 슬로우 모션 효과 후 게임 오버 패널 표시
        StartCoroutine(GameOverSequence());

        Debug.Log("Game Over!");
    }

    private IEnumerator GameOverSequence()
    {
        // 즉시 슬로우 모션 시작하고 서서히 Time.timeScale을 1로 복원
        Time.timeScale = slowMotionScale;
        float elapsedTime = 0f;
        while (elapsedTime < timeRecoveryDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / timeRecoveryDuration;
            Time.timeScale = Mathf.Lerp(slowMotionScale, 1f, t);
            yield return null;
        }

        // 정확히 1로 설정
        Time.timeScale = 1f;

        // 게임 오버 패널 표시
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 게임 오버 사운드 재생
        if (gameOverSound != null)
        {
            AudioSource.PlayClipAtPoint(gameOverSound, Camera.main.transform.position);
        }

        int finalScore = kangtoe99_ScoreSystem.Instance != null
            ? kangtoe99_ScoreSystem.Instance.GetCurrentScore()
            : 0;

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver(finalScore);
        }
    }

    private void RestartGame()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        isGameOver = false;
        isGameStarted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameStarted() => isGameStarted;
    public bool IsGameOver() => isGameOver;

    private void ToggleHelp()
    {
        if (helpObject == null) return;

        bool willBeActive = !helpObject.activeSelf;
        helpObject.SetActive(willBeActive);

        AudioClip clip = willBeActive ? helpOnSound : helpOffSound;
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}
