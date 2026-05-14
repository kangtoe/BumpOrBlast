using UnityEngine;
using UnityEngine.UI;

// 한 판 종합 정보 패널 (공용). 일시정지(PauseSystem)와 게임오버 1단계(GameManager)가
// 같은 패널 인스턴스를 공유한다 — 표시 내용(RunStats 수치 + 빌드)은 동일하고,
// 안내 문구(hintText)만 맥락에 따라 다르다.
// 종합 정보 스탯은 항목당 Text 1개 (VerticalLayoutGroup으로 행 배치) — 한 덩어리 문자열이 아니다.
// 패널을 켜고 끄는 진입점은 Show(hint) / Hide(). 켜질 때(OnEnable) 스냅샷을 갱신 —
// 일시정지/게임오버 중에는 수치가 변하지 않으므로 매 프레임 갱신 불필요.
public class kangtoe99_RunSummaryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot; // 켜고 끌 패널 최상위(배경 포함). 비우면 이 GameObject로 폴백.
    [SerializeField] private Text titleText;       // 맥락별 제목 (예: "Paused" / "Game Over")

    [Header("Summary (스탯별 별도 Text)")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text survivalText;
    [SerializeField] private Text killsText;
    [SerializeField] private Text damageDealtText;
    [SerializeField] private Text damageTakenText;

    [Header("Hint")]
    [SerializeField] private Text hintText; // 맥락별 안내 문구 (예: "ESC로 계속" / "Enter로 랭킹")

    [Header("Build")]
    [SerializeField] private kangtoe99_BuildDisplayUI buildDisplay;

    private GameObject Target => panelRoot != null ? panelRoot : gameObject;

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

        if (buildDisplay != null) buildDisplay.Refresh();
    }

    // "라벨  값" 형식으로 한 Text에 채운다.
    private static void SetRow(Text target, string label, string value)
    {
        if (target != null) target.text = $"{label}  {value}";
    }
}
