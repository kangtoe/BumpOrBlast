using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsData_New", menuName = "BumpOrBlast/PlayerStatsData", order = 0)]
public class kangtoe99_PlayerStatsData : ScriptableObject
{
    [Serializable]
    public struct StatEntry
    {
        public kangtoe99_StatType stat;
        public float value;
    }

    [SerializeField] private List<StatEntry> baseStats = new List<StatEntry>();

    public IReadOnlyList<StatEntry> BaseStats => baseStats;

    public void SetBaseStats(IEnumerable<KeyValuePair<kangtoe99_StatType, float>> values)
    {
        baseStats.Clear();
        foreach (var kv in values)
        {
            baseStats.Add(new StatEntry { stat = kv.Key, value = kv.Value });
        }
    }
}
