using UnityEngine;

// 한 판(run) 동안의 플레이 통계 누적.
// 게임 종료/일시정지 시 종합 정보 패널이 조회한다.
// 게임 시작~게임오버 사이에만 생존 시간을 누적.
public class kangtoe99_RunStats : MonoBehaviour
{
    public static kangtoe99_RunStats Instance { get; private set; }

    public float TotalDamageDealt { get; private set; }
    public float TotalDamageTaken { get; private set; }
    public int TotalKills { get; private set; }
    public float SurvivalTime { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Update()
    {
        var gm = kangtoe99_GameManager.Instance;
        if (gm == null) return;
        if (gm.IsGameStarted() && !gm.IsGameOver())
        {
            SurvivalTime += Time.deltaTime;
        }
    }

    public void AddDamageDealt(float amount)
    {
        if (amount > 0f) TotalDamageDealt += amount;
    }

    public void AddDamageTaken(float amount)
    {
        if (amount > 0f) TotalDamageTaken += amount;
    }

    public void AddKill()
    {
        TotalKills++;
    }

    // mm:ss 형식 생존 시간
    public string GetSurvivalTimeText()
    {
        int total = Mathf.FloorToInt(SurvivalTime);
        return $"{total / 60:00}:{total % 60:00}";
    }
}
