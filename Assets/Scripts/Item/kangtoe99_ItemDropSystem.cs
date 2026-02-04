using UnityEngine;

public class kangtoe99_ItemDropSystem : MonoBehaviour
{
    public static kangtoe99_ItemDropSystem Instance { get; private set; }

    [Header("Item Prefabs")]
    [SerializeField] private kangtoe99_ItemXPOrb xpOrbPrefab;
    [SerializeField] private kangtoe99_ItemHealthPack healthPackPrefab;
    [SerializeField] private kangtoe99_ItemBomb bombPrefab;

    [Header("Bonus Drop Rates (0-1)")]
    [SerializeField] private float healthPackDropRate = 0.1f; // 10%
    [SerializeField] private float bombDropRate = 0.02f;      // 2%

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TryDropItem(Vector3 position, Vector2 direction, int scoreValue)
    {
        // 아이템 확률 드롭
        float roll = Random.value;
        if (roll < bombDropRate)
        {
            SpawnBomb(position, direction);
        }
        else if (roll < bombDropRate + healthPackDropRate)
        {
            SpawnHealthPack(position, direction);
        }
        else
        {            
            // XP Orb는 기본 드롭        
            SpawnXPOrb(position, direction, scoreValue);
        }
    }

    private void SpawnXPOrb(Vector3 position, Vector2 direction, int scoreValue)
    {
        if (xpOrbPrefab == null) return;

        kangtoe99_ItemXPOrb xpOrb = Instantiate(xpOrbPrefab, position, Quaternion.identity);
        xpOrb.SetDropDirection(direction);
        xpOrb.SetScoreValue(scoreValue);
    }

    private void SpawnHealthPack(Vector3 position, Vector2 direction)
    {
        if (healthPackPrefab == null) return;

        kangtoe99_ItemHealthPack healthPack = Instantiate(healthPackPrefab, position, Quaternion.identity);
        healthPack.SetDropDirection(direction);
    }

    private void SpawnBomb(Vector3 position, Vector2 direction)
    {
        if (bombPrefab == null) return;

        kangtoe99_ItemBomb bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.SetDropDirection(direction);
    }
}
