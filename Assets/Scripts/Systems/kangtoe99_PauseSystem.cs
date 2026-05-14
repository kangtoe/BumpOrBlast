using UnityEngine;

// ESC 키 일시정지 토글. LevelUp 활성 중(timeScale=0 + 패널 없음)에는 무시.
public class kangtoe99_PauseSystem : MonoBehaviour
{
    public static kangtoe99_PauseSystem Instance { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    private bool isPaused;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (!CanToggleNow()) return;
        if (Input.GetKeyDown(toggleKey)) Toggle();
    }

    private bool CanToggleNow()
    {
        if (kangtoe99_GameManager.Instance == null) return false;
        if (!kangtoe99_GameManager.Instance.IsGameStarted()) return false;
        if (kangtoe99_GameManager.Instance.IsGameOver()) return false;
        // LevelUp이 timeScale=0 직접 설정 후 패널 표시 중인 동안엔 ESC 무시.
        // (이미 isPaused 상태면 무시 해제하기 위한 Resume은 허용)
        if (!isPaused && Time.timeScale == 0f) return false;
        return true;
    }

    public void Toggle()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void Resume()
    {
        if (!isPaused) return;
        Toggle();
    }
}
