using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Plant Defense/Data/Enemy Data")]
public class EnemyData : UnitData
{
    [Header("Enemy")]
    [Min(0f)]
    [SerializeField] private float moveSpeed = 1f; // Tốc độ di chuyển của enemy

    public override Faction Faction => global::Faction.Enemy;
    public override float MoveSpeed => moveSpeed;
}
