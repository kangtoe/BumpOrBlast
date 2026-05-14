using UnityEngine;

// LevelUp 폴백 선택지. 더 이상 영구 아이템 풀이 부족할 때 대신 등장.
// 선택 시 지정된 Drop prefab을 플레이어 위치에 잠시 인스턴스화한 뒤 픽업 효과를 즉시 발동.
[CreateAssetMenu(menuName = "BumpOrBlast/Instant Drop Item", fileName = "InstantDropItem_")]
public class kangtoe99_InstantDropItemData : ScriptableObject, kangtoe99_ILevelUpChoice
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [TextArea(2, 4)]
    [SerializeField] private string description;
    [SerializeField] private kangtoe99_Drop dropPrefab;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public Sprite Icon => icon;
    public string Description => description;

    public bool IsAvailable(GameObject player) => dropPrefab != null && player != null;

    public void Apply(GameObject player)
    {
        if (dropPrefab == null || player == null) return;
        var p = player.GetComponent<kangtoe99_Player>();
        if (p == null) return;

        var drop = Instantiate(dropPrefab, player.transform.position, Quaternion.identity);
        // 콜라이더로 인한 중복 픽업 방지: 활성 전 효과만 발동시키고 자기 자신 Destroy.
        drop.gameObject.SetActive(false);
        drop.TriggerPickup(p);
    }
}
