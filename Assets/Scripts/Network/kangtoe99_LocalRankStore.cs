using System.Collections.Generic;
using UnityEngine;

// 로컬(PlayerPrefs) 랭킹 스토어 — 서버의 /rank/around 미지원 + top 20 한계 때문에 임시로 로컬 전환.
// 서버 측 엔드포인트 추가 후 RankApi 경로로 복귀 예정 (Docs/SystemsRework.md 2026-05-19 항목 참고).
public static class kangtoe99_LocalRankStore
{
    private const string PrefsKey = "BumpOrBlast.LocalRanks.v1";

    [System.Serializable]
    private class Store
    {
        public List<RankData> entries = new List<RankData>();
        public int nextId = 1;
    }

    private static Store cache;

    private static Store Load()
    {
        if (cache != null) return cache;

        string json = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(json))
        {
            cache = new Store();
            return cache;
        }

        Store loaded = null;
        try { loaded = JsonUtility.FromJson<Store>(json); }
        catch { loaded = null; }

        cache = loaded ?? new Store();
        if (cache.entries == null) cache.entries = new List<RankData>();
        if (cache.nextId <= 0) cache.nextId = 1;
        return cache;
    }

    private static void SaveCache()
    {
        if (cache == null) return;
        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(cache));
        PlayerPrefs.Save();
    }

    public static RankData Create(int level, string name, int score)
    {
        var s = Load();
        var entry = new RankData
        {
            id = s.nextId++,
            level = level,
            name = name,
            score = score
        };
        s.entries.Add(entry);
        SaveCache();
        return entry;
    }

    // 점수 내림차순 정렬된 전체 사본.
    public static RankData[] GetAllSorted()
    {
        var s = Load();
        var copy = new List<RankData>(s.entries);
        copy.Sort((a, b) => b.score.CompareTo(a.score));
        return copy.ToArray();
    }

    public static void UpdateName(int id, string newName)
    {
        var s = Load();
        for (int i = 0; i < s.entries.Count; i++)
        {
            if (s.entries[i].id == id)
            {
                s.entries[i].name = newName;
                SaveCache();
                return;
            }
        }
    }
}
