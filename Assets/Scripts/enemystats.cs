using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Movement & Vision")]
    public float moveSpeed = 3f;
    public float visionRange = 6f;
    public float attackRange = 1.5f;

    [Header("Combat")]
    public int damage = 10;
    public float windUpDuration = 0.5f; // Время замаха (для тарана или слайма)
    public float stunDuration = 1.5f;   // Время оглушения (для тарана при ударе о стену)
}