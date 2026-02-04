using UnityEngine;

public class kangtoe99_ItemHealthPack : kangtoe99_Item
{
    [Header("Health Pack Settings")]
    [SerializeField] private float healAmount = 50f;

    protected override void OnPickup(kangtoe99_Player player)
    {
        player.Heal(healAmount);
        Debug.Log($"Health Pack picked up! +{healAmount} HP");
    }
}
