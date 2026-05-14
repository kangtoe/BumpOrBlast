using System.Collections.Generic;
using System.Text;
using UnityEngine;

// 백틱(`) 키 토글 디버그 패널.
// 예외적으로 IMGUI(OnGUI) 사용 — Canvas/prefab/Button 위젯 없이 GUILayout으로 즉시 그림.
// Time.timeScale은 건드리지 않음(게임 진행 중에도 사용 가능).
public class kangtoe99_DebugPanel : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
    [SerializeField, Min(0.05f)] private float refreshInterval = 0.25f;
    [SerializeField] private Vector2 panelPosition = new Vector2(20, 20);
    [SerializeField] private Vector2 panelSize = new Vector2(260, 280);

    private bool isOpen;
    private float nextRefreshAt;
    private string cachedInfo = string.Empty;

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey)) isOpen = !isOpen;
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        if (Time.unscaledTime >= nextRefreshAt)
        {
            cachedInfo = BuildInfoText();
            nextRefreshAt = Time.unscaledTime + refreshInterval;
        }

        var rect = new Rect(panelPosition.x, panelPosition.y, panelSize.x, panelSize.y);
        GUI.Box(rect, "Debug ( ` )");

        GUILayout.BeginArea(new Rect(rect.x + 8, rect.y + 24, rect.width - 16, rect.height - 32));
        GUILayout.Label(cachedInfo);
        GUILayout.Space(6);
        if (GUILayout.Button("Force Level Up")) Action_ForceLevelUp();
        if (GUILayout.Button("Kill All Enemies")) Action_KillAllEnemies();
        if (GUILayout.Button("Heal Full")) Action_HealFull();
        if (GUILayout.Button("Add Random Item")) Action_AddRandomItem();
        GUILayout.EndArea();
    }

    // ─── 정보 ──────────────────────────────────────────────────

    private string BuildInfoText()
    {
        var sb = new StringBuilder();

        int level = kangtoe99_LevelUpSystem.Instance != null
            ? kangtoe99_LevelUpSystem.Instance.GetCurrentLevel() : 0;
        int score = kangtoe99_ScoreSystem.Instance != null
            ? kangtoe99_ScoreSystem.Instance.GetCurrentScore() : 0;
        int nextScore = kangtoe99_LevelUpSystem.Instance != null
            ? kangtoe99_LevelUpSystem.Instance.GetNextLevelScore() : 0;
        sb.Append("Lv ").Append(level)
          .Append("  Score ").Append(score).Append(" / ").Append(nextScore).AppendLine();

        var player = FindFirstObjectByType<kangtoe99_Player>();
        if (player != null)
        {
            sb.Append("HP ").Append(player.GetCurrentHealth().ToString("F0"))
              .Append(" / ").Append(player.GetMaxHealth().ToString("F0"));
        }
        var energy = FindFirstObjectByType<kangtoe99_EnergySystem>();
        if (energy != null)
        {
            sb.Append("  EN ").Append(energy.Current.ToString("F0"))
              .Append(" / ").Append(energy.Max.ToString("F0"));
        }
        sb.AppendLine();

        int enemyCount = FindObjectsByType<kangtoe99_Enemy>(FindObjectsSortMode.None).Length;
        sb.Append("Enemies ").Append(enemyCount).AppendLine();

        if (player != null)
        {
            var inv = player.GetComponent<kangtoe99_ItemInventory>();
            if (inv != null) sb.Append("Items ").Append(inv.EntryCount);
        }

        return sb.ToString();
    }

    // ─── 액션 ──────────────────────────────────────────────────

    private void Action_ForceLevelUp()
    {
        if (kangtoe99_LevelUpSystem.Instance == null || kangtoe99_ScoreSystem.Instance == null) return;
        int need = kangtoe99_LevelUpSystem.Instance.GetNextLevelScore() - kangtoe99_ScoreSystem.Instance.GetCurrentScore();
        if (need > 0) kangtoe99_ScoreSystem.Instance.AddScore(need);
    }

    private void Action_KillAllEnemies()
    {
        foreach (var e in FindObjectsByType<kangtoe99_Enemy>(FindObjectsSortMode.None))
        {
            if (e != null) e.TakeDamage(float.MaxValue);
        }
    }

    private void Action_HealFull()
    {
        var player = FindFirstObjectByType<kangtoe99_Player>();
        if (player == null) return;
        float missing = player.GetMaxHealth() - player.GetCurrentHealth();
        if (missing > 0) player.Heal(missing);
    }

    private void Action_AddRandomItem()
    {
        var player = FindFirstObjectByType<kangtoe99_Player>();
        if (player == null) return;
        var inv = player.GetComponent<kangtoe99_ItemInventory>();
        if (inv == null || kangtoe99_LevelUpSystem.Instance == null) return;
        var pool = kangtoe99_LevelUpSystem.Instance.GetItemPoolSnapshot();
        if (pool == null) return;

        var available = new List<kangtoe99_ItemData>();
        for (int i = 0; i < pool.Count; i++)
        {
            var item = pool[i];
            if (item != null && !inv.IsFull(item)) available.Add(item);
        }
        if (available.Count == 0)
        {
            Debug.Log("[DebugPanel] 추가 가능한 아이템 없음");
            return;
        }
        inv.TryAdd(available[Random.Range(0, available.Count)]);
    }
}
