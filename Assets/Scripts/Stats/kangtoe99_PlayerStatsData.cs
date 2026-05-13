using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData_New", menuName = "BumpOrBlast/PlayerStatsData", order = 0)]
public class kangtoe99_PlayerStatsData : ScriptableObject
{
    [SerializeField] private kangtoe99_StatMap baseStats = new kangtoe99_StatMap();

    public kangtoe99_StatMap BaseStats
    {
        get
        {
            EnsureInitialized();
            return baseStats;
        }
    }

    private void OnEnable()
    {
        EnsureInitialized();
    }

    private void OnValidate()
    {
        baseStats?.EnsureSize();
    }

    // 자산이 처음 생성됐거나 enum 항목이 추가됐을 때 코드 Defaults로 초기화/보강.
    private void EnsureInitialized()
    {
        if (baseStats == null)
        {
            baseStats = new kangtoe99_StatMap();
            baseStats.CopyFrom(kangtoe99_PlayerStats.Defaults);
            return;
        }

        int prevCount = baseStats.Count;
        baseStats.EnsureSize();

        // 새 자산: 모든 항목이 0이면 Defaults로 채움
        if (prevCount == 0)
        {
            baseStats.CopyFrom(kangtoe99_PlayerStats.Defaults);
        }
    }
}
