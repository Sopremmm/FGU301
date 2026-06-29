using UnityEngine;

public abstract class UnitData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string unitName; // Tên hiển thị của unit
    [TextArea]
    [SerializeField] private string description; // Mô tả unit
    [SerializeField] private Sprite icon; // Icon dạng sprite
    [SerializeField] private RenderTexture renderTexture; // Icon dạng render texture
    [SerializeField] private GameObject prefab; // Prefab dùng để spawn unit

    [Header("Combat")]
    [Min(1)]
    [SerializeField] private int maxHealth = 100; // Máu tối đa
    [Min(0)]
    [SerializeField] private int damage = 10; // Sát thương cơ bản
    [Min(0f)]
    [SerializeField] private float attackRange = 1f; // Tầm đánh
    [Min(0.01f)]
    [SerializeField] private float attackCooldown = 1f; // Thời gian nghỉ sau mỗi lần đánh
    [SerializeField] private AttackType attackType = AttackType.Melee; // Loại tấn công

    [Header("Movement")]
    [SerializeField] private MovementType movementType = MovementType.None; // Kiểu di chuyển

    [Header("Ranged Attack")]
    [SerializeField] private Projectile projectilePrefab; // Prefab đạn cho unit đánh xa

    public string UnitName => unitName;
    public string Description => description;
    public Sprite Icon => icon;
    public RenderTexture RenderTexture => renderTexture;
    public GameObject Prefab => prefab;
    public abstract Faction Faction { get; }
    public int MaxHealth => maxHealth;
    public int Damage => damage;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public AttackType AttackType => attackType;
    public MovementType MovementType => movementType;
    public Projectile ProjectilePrefab => projectilePrefab;
    public virtual float MoveSpeed => 0f;
}
