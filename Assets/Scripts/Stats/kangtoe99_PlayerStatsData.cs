using UnityEngine;

// 플레이어 기본 스탯의 진리원천. 값은 자산 인스펙터에서 직접 입력.
// 코드는 기본값을 제공하지 않으며 새 자산은 모든 stat이 0으로 시작.
[CreateAssetMenu(fileName = "PlayerStatsData_New", menuName = "BumpOrBlast/PlayerStatsData", order = 0)]
public class kangtoe99_PlayerStatsData : ScriptableObject
{
    [SerializeField] private kangtoe99_StatMap baseStats = new kangtoe99_StatMap();

    public kangtoe99_StatMap BaseStats
    {
        get
        {
            baseStats?.EnsureSize();
            return baseStats;
        }
    }

    private void OnValidate()
    {
        baseStats?.EnsureSize();
    }
}
