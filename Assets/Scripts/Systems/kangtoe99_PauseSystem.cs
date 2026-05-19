using UnityEngine;

// ESC 키 일시정지 토글. LevelUp 활성 중(timeScale=0 + 패널 없음)에는 무시.
// 일시정지 화면은 공용 종합 정보 패널(kangtoe99_RunSummaryUI)을 띄운다 — 게임오버 1단계와 같은 인스턴스.
public class kangtoe99_PauseSystem : MonoBehaviour
{
    public static kangtoe99_PauseSystem Instance { get; private set; }

    [SerializeField] private kangtoe99_RunSummaryUI infoPanel; // 일시정지/게임오버 공용 정보 패널
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private string pauseTitle = "Paused";
    [SerializeField] private string pauseHint = "Press ESC to Resume";

    private bool isPaused;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (infoPanel != null) infoPanel.Hide();
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
        // 이름 입력 패널이 열려 있는 동안엔 ESC를 무시 (입력 패널의 Confirm/Cancel로만 닫는다).
        if (infoPanel != null && infoPanel.IsNameEditOpen) return false;
        // LevelUp이 timeScale=0 직접 설정 후 패널 표시 중인 동안엔 ESC 무시.
        // (이미 isPaused 상태면 무시 해제하기 위한 Resume은 허용)
        if (!isPaused && Time.timeScale == 0f) return false;
        // LevelUp 이 같은 프레임에 ESC 로 패널을 닫았다면 같이 발생한 이 ESC 는 소비된 것으로 보고 무시.
        // (스크립트 실행 순서에 따라 LevelUpSystem 이 먼저 돌면 timeScale 이 1 로 풀려 위 가드를 통과해버림)
        if (kangtoe99_LevelUpSystem.LastEscapeConsumedFrame == Time.frameCount) return false;
        return true;
    }

    public void Toggle()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (kangtoe99_GameManager.Instance != null)
            kangtoe99_GameManager.Instance.SetHudVisible(!isPaused);

        if (infoPanel == null) return;
        if (isPaused) infoPanel.Show(pauseTitle, pauseHint);
        else infoPanel.Hide();
    }

    public void Resume()
    {
        if (!isPaused) return;
        Toggle();
    }
}
