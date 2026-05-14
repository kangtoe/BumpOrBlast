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
        foreach (var kv in entries)
            yield return new BuildEntry(kv.Key, kv.Value.stack);
    }

    public int EntryCount => entries.Count;

    public bool IsFull(kangtoe99_ItemData data)
    {
        if (data == null) return true;
        return GetStack(data) >= data.MaxStack;
    }

    public bool TryAdd(kangtoe99_ItemData data)
    {
        if (data == null || stats == null) return false;
        if (!entries.TryGetValue(data, out var entry))
        {
            entry = new Entry { data = data };
            entries[data] = entry;
        }
        if (entry.stack >= data.MaxStack)
        {
            Debug.Log($"[ItemInventory] {data.DisplayName} 보유 한도({data.MaxStack}) 도달 — 추가 안함");
            return false;
        }

        var modSource = entry; // 스택 단위 source는 Entry 객체 자체
        for (int i = 0; i < data.Modifiers.Count; i++)
        {
            var mod = data.Modifiers[i].CreateModifier(modSource);
            entry.modifiers.Add(mod);
            stats.AddModifier(mod);
        }
        entry.stack++;
        OnItemAdded?.Invoke(data, entry.stack);
        Debug.Log($"[ItemInventory] +{data.DisplayName} (스택 {entry.stack}/{data.MaxStack}, modifier {data.Modifiers.Count}개 추가)");
        return true;
    }
}
