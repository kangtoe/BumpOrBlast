using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kangtoe99_ItemBomb : kangtoe99_Item
{
    [Header("Bomb Settings")]
    [SerializeField] private float delayBetweenKills = 0.05f;

    protected override void OnPickup(kangtoe99_Player player)
    {
        Vector3 bombPosition = transform.position;
        player.StartCoroutine(KillEnemiesSequentially(bombPosition));
    }

    private IEnumerator KillEnemiesSequentially(Vector3 bombPosition)
    {
        List<GameObject> enemies = new(GameObject.FindGameObjectsWithTag("Enemy"));
        enemies.Sort((a, b) =>
            Vector3.Distance(a.transform.position, bombPosition)
            .CompareTo(Vector3.Distance(b.transform.position, bombPosition)));

        int killCount = 0;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            if (enemy.TryGetComponent(out kangtoe99_Enemy enemyScript))
            {
                enemyScript.TakeDamage(float.MaxValue);
                killCount++;
                yield return new WaitForSeconds(delayBetweenKills);
            }
        }

        Debug.Log($"Bomb activated! Killed {killCount} enemies");
    }
}
