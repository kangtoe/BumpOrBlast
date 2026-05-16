using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "kangtoe99/Enemy Data")]
public class kangtoe99_EnemyData : ScriptableObject
{
    [Header("Enemy Type")]
    public string enemyName = "Normal";

    [Header("Stats")]
    public float maxHealth = 50f;
    public float damage = 10f;
    public int scoreValue = 10;

    [Header("Movement & Physics")]
    public float moveForce = 2f;
    public float mass = 1f;
    public float linearDamping = 1f;
    public float maxRotationSpeed = 180f;
    public float speedCapOvershoot = 1.5f;
    public float collisionKnockbackForce = 0.8f;
}
