using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 한 판 종합 정보 패널 (공용). 일시정지(PauseSystem)와 게임오버 1단계(GameManager)가
// 같은 패널 인스턴스를 공유한다 — 표시 내용(RunStats 수치 + 빌드)은 동일하고,
// 안내 문구(hintText)만 맥락에 따라 다르다.
// 종합 정보 스탯은 항목당 Text 1개 (VerticalLayoutGroup으로 행 배치) — 한 덩어리 문자열이 아니다.
// 패널을 켜고 끄는 진입점은 Show(hint) / Hide(). 켜질 때(OnEnable) 스냅샷을 갱신 —
// 일시정지/게임오버 중에는 수치가 변하지 않으므로 매 프레임 갱신 불필요.
// 플레이어 이름은 여기서 보여주고 수정한다 — Edit 버튼 → 입력 패널. 로컬(PlayerPrefs) 저장이며,
// 게임오버 중 수정 시 서버 랭킹 항목 이름도 갱신한다.
public class kangtoe99_RunSummaryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot; // 켜고 끌 패널 최상위(배경 포함). 비우면 이 GameObject로 폴백.
    [SerializeField] private TMP_Text titleText;       // 맥락별 제목 (예: "Paused" / "Game Over")

    [Header("Summary (스탯별 별도 Text)")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text survivalText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text damageDealtText;
    [SerializeField] private TMP_Text damageTakenText;

    [Header("Hint")]
    [SerializeField] private TMP_Text hintText; // 맥락별 안내 문구 (예: "ESC로 계속" / "Enter로 랭킹")

    [Header("Build")]
    [SerializeField] private kangtoe99_BuildDisplayUI buildDisplay;

    [Header("Player Name")]
    [SerializeField] private TMP_Text nameText;                 // 이름 표시 행
    [SerializeField] private Button editNameButton;         // 누르면 입력 패널 표시
    [SerializeField] private GameObject nameEditPanel;      // 이름 입력 패널 (InputField + 확인/취소)
    [SerializeField] private TMP_InputField nameEditInput;
    [SerializeField] private Button nameEditConfirmButton;
    [SerializeField] private Button nameEditCancelButton;

    private GameObject Target => panelRoot != null ? panelRoot : gameObject;

    // 이름 입력 패널이 열려 있는 동안엔 Enter/Space/ESC 같은 키가 게임 진행(랭킹 전환·일시정지 해제)에
    // 쓰이지 않도록 — 입력 중 스페이스/엔터를 이름에 쓸 수 있어야 한다.
    public bool IsNameEditOpen => nameEditPanel != null && nameEditPanel.activeSelf;

    private void Awake()
    {
        if (editNameButton != null) editNameButton.onClick.AddListener(OpenNameEdit);
        if (nameEditConfirmButton != null) nameEditConfirmButton.onClick.AddListener(ConfirmNameEdit);
        if (nameEditCancelButton != null) nameEditCancelButton.onClick.AddListener(CloseNameEdit);
    }

    private void OnEnable()
    {
        Refresh();
    }

    // 맥락(일시정지/게임오버)에 맞는 제목·안내 문구로 패널을 켠다.
    public void Show(string title, string hint)
    {
        if (titleText != null) titleText.text = title;
        if (hintText != null) hintText.text = hint;
        Target.SetActive(true);   // 자식 활성화 → OnEnable → Refresh
        Refresh();                // 이미 켜져 있던 경우에도 최신 스냅샷 보장
    }

    public void Hide()
    {
        Target.SetActive(false);
    }

    public void Refresh()
    {
        var levelSys = kangtoe99_LevelUpSystem.Instance;
        SetRow(levelText, "Level", levelSys != null ? levelSys.GetCurrentLevel().ToString() : "-");

        var scoreSys = kangtoe99_ScoreSystem.Instance;
        SetRow(scoreText, "Score", scoreSys != null ? scoreSys.GetCurrentScore().ToString("N0") : "-");

        var stats = kangtoe99_RunStats.Instance;
        SetRow(survivalText, "Survival Time", stats != null ? stats.GetSurvivalTimeText() : "-");
        SetRow(killsText, "Kills", stats != null ? stats.TotalKills.ToString() : "-");
        SetRow(damageDealtText, "Damage Dealt", stats != null ? Mathf.RoundToInt(stats.TotalDamageDealt).ToString("N0") : "-");
        SetRow(damageTakenText, "Damage Taken", stats != null ? Mathf.RoundToInt(stats.TotalDamageTaken).ToString("N0") : "-");

        RefreshNameRow();

        // 패널이 새로 열릴 때는 이름 입력 패널은 항상 닫힌 상태로 시작.
        if (nameEditPanel != null) nameEditPanel.SetActive(false);

        if (buildDisplay != null) buildDisplay.Refresh();
    }

    private void RefreshNameRow()
    {
        string playerName = kangtoe99_GameManager.Instance != null
            ? kangtoe99_GameManager.Instance.PlayerName
            : "-";
        SetRow(nameText, "Name", playerName);
    }

    // Edit 버튼 → 현재 이름을 채운 입력 패널을 연다.
    private void OpenNameEdit()
    {
        if (nameEditPanel == null) return;

        if (nameEditInput != null)
        {
            nameEditInput.text = kangtoe99_GameManager.Instance != null
                ? kangtoe99_GameManager.Instance.PlayerName
                : "";
        }
        nameEditPanel.SetActive(true);
        if (nameEditInput != null) nameEditInput.Select();
    }

    // 확인 → 로컬 저장 + (게임오버 중이면) 서버 랭킹 항목 이름 갱신.
    private void ConfirmNameEdit()
    {
        var gm = kangtoe99_GameManager.Instance;
        if (gm != null && nameEditInput != null)
        {
            gm.SetPlayerName(nameEditInput.text);

            // 게임오버 중이면 이미 제출된 내 랭킹 항목의 이름도 갱신.
            if (gm.IsGameOver() && kangtoe99_GameOverUI.Instance != null)
            {
                kangtoe99_GameOverUI.Instance.UpdateMyRankName(gm.PlayerName);
            }
        }

        RefreshNameRow();
        CloseNameEdit();
    }

    private void CloseNameEdit()
    {
        if (nameEditPanel != null) nameEditPanel.SetActive(false);
    }

    // "라벨  값" 형식으로 한 Text에 채운다.
    private static void SetRow(TMP_Text target, string label, string value)
    {
        if (target != null) target.text = $"{label}  {value}";
    }
}
