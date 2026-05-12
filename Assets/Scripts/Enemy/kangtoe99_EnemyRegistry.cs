using System.Collections.Generic;
using UnityEngine;

public static class kangtoe99_EnemyRegistry
{
    private static readonly List<kangtoe99_Enemy> activeEnemies = new List<kangtoe99_Enemy>();

    public static IReadOnlyList<kangtoe99_Enemy> ActiveEnemies => activeEnemies;
    public static int Count => activeEnemies.Count;

    public static void Register(kangtoe99_Enemy enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public static void Unregister(kangtoe99_Enemy enemy)
    {
        activeEnemies.Remove(enemy);
    }

    // 가장 가까운 적 찾기 (자동 조준 무기용). maxDistance 초과하면 null.
    public static kangtoe99_Enemy FindNearest(Vector2 position, float maxDistance = float.MaxValue)
    {
        kangtoe99_Enemy nearest = null;
        float nearestDistSq = maxDistance * maxDistance;
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            var e = activeEnemies[i];
            if (e == null) continue;
            float dSq = ((Vector2)e.transform.position - position).sqrMagnitude;
            if (dSq < nearestDistSq)
            {
                nearestDistSq = dSq;
                nearest = e;
            }
        }
        return nearest;
    }

    // Domain Reload 비활성화 환경에서 정적 상태가 플레이 세션 간 유지되는 것 방지
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLoad()
    {
        activeEnemies.Clear();
    }
}
