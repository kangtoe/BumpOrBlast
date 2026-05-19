using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using System.Collections;

public class kangtoe99_GameManager : MonoBehaviour
{
    public static kangtoe99_GameManager Instance { get; private set; }

    [Header("Game State")]
    private bool isGameStarted = false;
    private bool isGameOver = false;

    [Header("UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private kangtoe99_RunSummaryUI infoPanel; // 게임오버 1단계: 공용 종합 정보 창 (일시정지와 공유)
    [SerializeField] private GameObject gameOverPanel; // 게임오버 2단계: 랭킹(리더보드)
    [SerializeField] private kangtoe99_GameOverUI gameOverUI;
    [SerializeField] private GameObject hudPanel; // 인게임 HUD — 게임 플레이 중에만 표시

    // 이름은 시작 화면이 아니라 종합 정보 패널(RunSummaryUI)에서 보고 수정한다.
    // 로컬(PlayerPrefs)에 저장돼 다음 실행에도 유지된다.
    private const string PlayerNamePrefKey = "PlayerName";
    private const string DefaultPlayerName = "player name";
    public string PlayerName { get; private set; }

    [Header("Game Over Settings")]
    [Tooltip("플레이어 격파 후 정보 패널이 뜨기까지의 호흡 시간(실시간).")]
    [FormerlySerializedAs("timeRecoveryDuration")]
    [SerializeField] private float gameOverDelay = 3f;
    [SerializeField] private string summaryTitle = "Game Over";
    [SerializeField] private string summaryHint = "Press Enter / Space for Ranking";

    [Header("Help")]
    [SerializeField] private GameObject helpObject;
    [SerializeField] private KeyCode helpToggleKey = KeyCode.H;
    [SerializeField] private AudioClip helpOnSound;
    [SerializeField] private AudioClip helpOffSound;

    [Header("SFX")]
    [SerializeField] private AudioClip sceneStartSound;
    [SerializeField] private AudioClip gameStartSound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Game Start FX")]
    [SerializeField] private string startFxText = "START!";
    [SerializeField] private Color startFxColor = Color.white;
    [SerializeField] private float startFxBurstRadius = 3f;
    [SerializeField] private float startFxBurstKnockback = 12f;

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

        // 로컬에 저장된 이름을 불러온다 (없으면 기본값)
        PlayerName = PlayerPrefs.GetString(PlayerNamePrefKey, DefaultPlayerName);

        // 시작 패널 표시
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        // 게임오버 관련 패널 숨기기 (정보 창 → 랭킹 2단계)
        if (infoPanel != null)
        {
            infoPanel.Hide();
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 시작 전엔 HUD 숨김 — StartGame 에서 표시
        SetHudVisible(false);

        // 슬롯 prefab 인스턴스화 비용을 로딩 구간에서 미리 지불 — 게임오버/일시정지
        // 패널 첫 활성화 시 발생하는 hitch 완화. 비활성 패널 내부 컴포넌트도 포함해 찾는다.
        WarmUpBuildDisplays();
    }

    private void WarmUpBuildDisplays()
    {
        var displays = FindObjectsByType<kangtoe99_BuildDisplayUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < displays.Length; i++)
        {
            var d = displays[i];
            if (d == null) continue;
            d.WarmUp(d.Columns * d.Rows);
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

    // 종합 정보 패널(RunSummaryUI)의 이름 편집에서 호출. 로컬에 저장해 다음 실행에도 유지한다.
    public void SetPlayerName(string newName)
    {
        string trimmed = newName != null ? newName.Trim() : "";
        PlayerName = string.IsNullOrEmpty(trimmed) ? DefaultPlayerName : trimmed;
        PlayerPrefs.SetString(PlayerNamePrefKey, PlayerName);
        PlayerPrefs.Save();
    }

    private void StartGame()
    {
        isGameStarted = true;

        // 게임 시작 사운드 재생
        if (gameStartSound != null)
        {
            AudioSource.PlayClipAtPoint(gameStartSound, Camera.main.transform.position);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        SetHudVisible(true);

        // 적 스포너 시작
        if (kangtoe99_EnemySpawner.Instance != null)
        {
            kangtoe99_EnemySpawner.Instance.StartSpawning();
        }

        EmitStartFx();

        Debug.Log("Game Started!");
    }

    private void EmitStartFx()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        Vector2 pos = player.transform.position;

        if (kangtoe99_FloatingTextManager.Instance != null)
            kangtoe99_FloatingTextManager.Instance.ShowAtPlayer(startFxText, startFxColor);

        if (kangtoe99_ExplosionManager.Instance != null)
            kangtoe99_ExplosionManager.Instance.SpawnOne(pos, 0f, startFxBurstRadius, startFxBurstKnockback, startFxColor);
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

        SetHudVisible(false);

        // 격파 후 호흡 시간을 두고 정보 패널을 띄운다 (슬로모션 없음).
        StartCoroutine(GameOverSequence());

        Debug.Log("Game Over!");
    }

    private IEnumerator GameOverSequence()
    {
        // 실시간 딜레이 — Time.timeScale 은 건드리지 않는다.
        yield return new WaitForSecondsRealtime(gameOverDelay);

        // 게임 오버 사운드 재생
        if (gameOverSound != null)
        {
            AudioSource.PlayClipAtPoint(gameOverSound, Camera.main.transform.position);
        }

        int finalScore = kangtoe99_ScoreSystem.Instance != null
            ? kangtoe99_ScoreSystem.Instance.GetCurrentScore()
            : 0;

        // 랭킹 서버 요청을 먼저 시작해 둔다. 플레이어가 정보 창을 읽는 동안
        // 백그라운드로 리더보드가 채워져, 전환 시점엔 거의 준비된 상태가 된다.
        // (gameOverUI는 항상 활성인 루트 오브젝트라 패널 비활성과 무관하게 코루틴이 돈다)
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver(finalScore);
        }

        // 1단계: 공용 종합 정보 창 표시 → Enter/Space 입력 대기
        if (infoPanel != null)
        {
            infoPanel.Show(summaryTitle, summaryHint);

            // 이름 입력 패널이 열려 있는 동안엔 Enter/Space를 이름 입력에 양보한다.
            yield return new WaitUntil(() =>
                !infoPanel.IsNameEditOpen &&
                (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)));

            infoPanel.Hide();
        }

        // 2단계: 랭킹(리더보드) 패널 표시
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
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

    // 일시정지/레벨업 등 게임 흐름이 멈추는 상태에서 HUD 를 토글하기 위한 외부 진입점.
    public void SetHudVisible(bool visible)
    {
        if (hudPanel != null) hudPanel.SetActive(visible);
    }

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
