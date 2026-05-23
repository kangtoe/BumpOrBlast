using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(kangtoe99_PlayerStats))]
public class kangtoe99_ItemInventory : MonoBehaviour
{
    private class Entry
    {
        public kangtoe99_ItemData data;
        public int stack;
        public readonly List<kangtoe99_IStatModifier> modifiers = new List<kangtoe99_IStatModifier>();
    }

    private kangtoe99_PlayerStats stats;
    private readonly Dictionary<kangtoe99_ItemData, Entry> entries = new Dictionary<kangtoe99_ItemData, Entry>();
    // 획득 순서를 명시적으로 유지 — Dictionary iteration 순서에 의존하지 않음
    private readonly List<kangtoe99_ItemData> order = new List<kangtoe99_ItemData>();
    // 티어별 현재 스택 합계 캐시. AddOnly 전제 — Remove는 현재 없음.
    // 상한값은 kangtoe99_TierStackLimits에서 읽음 (단일 출처, ItemData.MaxStack과 공유).
    private readonly int[] tierStackCount = new int[5];

    public event Action<kangtoe99_ItemData, int> OnItemAdded;

    private void Awake()
    {
        stats = GetComponent<kangtoe99_PlayerStats>();
    }

    public int GetStack(kangtoe99_ItemData data)
    {
        if (data == null) return 0;
        return entries.TryGetValue(data, out var e) ? e.stack : 0;
    }

    public readonly struct BuildEntry
    {
        public readonly kangtoe99_ItemData data;
        public readonly int stack;
        public BuildEntry(kangtoe99_ItemData d, int s) { data = d; stack = s; }
    }

    public IEnumerable<BuildEntry> GetBuildEntries()
    {
        for (int i = 0; i < order.Count; i++)
        {
            var data = order[i];
            if (entries.TryGetValue(data, out var e))
                yield return new BuildEntry(data, e.stack);
        }
    }

    public int EntryCount => entries.Count;

    public bool IsFull(kangtoe99_ItemData data)
    {
        if (data == null) return true;
        int tierIdx = (int)data.Tier;
        if (tierIdx < 0 || tierIdx >= tierStackCount.Length) return true;
        return tierStackCount[tierIdx] >= kangtoe99_TierStackLimits.GetLimit(data.Tier);
    }

    public int GetTierStackLimit(kangtoe99_Tier tier) => kangtoe99_TierStackLimits.GetLimit(tier);

    public int GetTierStackCount(kangtoe99_Tier tier)
    {
        int i = (int)tier;
        return (i >= 0 && i < tierStackCount.Length) ? tierStackCount[i] : 0;
    }

    public bool TryAdd(kangtoe99_ItemData data)
    {
        if (data == null || stats == null) return false;

        int tierIdx = (int)data.Tier;
        if (tierIdx < 0 || tierIdx >= tierStackCount.Length) return false;
        int tierLimit = kangtoe99_TierStackLimits.GetLimit(data.Tier);
        if (tierStackCount[tierIdx] >= tierLimit)
        {
            Debug.Log($"[ItemInventory] {data.DisplayName} 추가 불가 — 티어 {data.Tier} 한도({tierLimit}) 도달");
            return false;
        }

        if (!entries.TryGetValue(data, out var entry))
        {
            entry = new Entry { data = data };
            entries[data] = entry;
            order.Add(data);
        }

        var modSource = entry; // 스택 단위 source는 Entry 객체 자체
        for (int i = 0; i < data.Modifiers.Count; i++)
        {
            var mod = data.Modifiers[i].CreateModifier(modSource);
            entry.modifiers.Add(mod);
            stats.AddModifier(mod);
        }
        entry.stack++;
        tierStackCount[tierIdx]++;
        OnItemAdded?.Invoke(data, entry.stack);
        Debug.Log($"[ItemInventory] +{data.DisplayName} (티어 {data.Tier} {tierStackCount[tierIdx]}/{tierLimit}, modifier {data.Modifiers.Count}개 추가)");
        return true;
    }
}
