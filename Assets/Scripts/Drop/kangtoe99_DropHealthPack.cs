using UnityEngine;

public class kangtoe99_DropHealthPack : kangtoe99_Drop
{
    [Header("Health Pack Settings")]
    [SerializeField] private float healAmount = 50f;

    [Header("Push Burst (피해 0의 녹색 폭발 — 적을 밀어내고 시각 효과)")]
    [Tooltip("플레이어 위치에서 터지는 폭발 반경.")]
    [SerializeField] private float burstRadius = 3f;
    [Tooltip("폭발 임펄스 — 적을 밀어내는 힘.")]
    [SerializeField] private float burstKnockback = 12f;
    [Tooltip("폭발 VFX 색상. 끄면 Drop의 SpriteRenderer 색상을 따른다.")]
    [SerializeField] private bool overrideColor = false;
    [Tooltip("overrideColor가 켜졌을 때만 사용.")]
    [SerializeField] private Color burstColor = new Color(0.2f, 1f, 0.5f, 1f);

    protected override void OnPickup(kangtoe99_Player player)
    {
        player.Heal(healAmount);

        if (kangtoe99_ExplosionManager.Instance != null)
        {
            Color color = overrideColor ? burstColor : GetRendererColor();
            color.a = 1f;
            kangtoe99_ExplosionManager.Instance.SpawnOne(
                player.transform.position, 0f, burstRadius, burstKnockback, color);
        }

        Debug.Log($"Health Pack picked up! +{healAmount} HP, push burst r={burstRadius}");
    }

    private Color GetRendererColor()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.color : burstColor;
    }
}
