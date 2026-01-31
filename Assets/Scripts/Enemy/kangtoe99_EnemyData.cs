using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "kangtoe99/Enemy Data")]
public class kangtoe99_EnemyData : ScriptableObject
{
    [Header("Enemy Type")]
    public string enemyName = "Normal";

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float maxHealth = 50f;
    public float damage = 10f;
    public int scoreValue = 10;

    [Header("Visual")]
    public Sprite sprite;
    public Color color = Color.white;
}
