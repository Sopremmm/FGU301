using UnityEngine;

[RequireComponent(typeof(HealthModule))]
public class UnitController : MonoBehaviour
{
    [SerializeField] private UnitData data; // Data cấu hình unit

    [SerializeField] private int laneIndex = -1; // Lane hiện tại của unit

    private Faction faction; // Phe của unit
    private HealthModule healthModule; // Module quản lý máu
    private int maxHealth; // Máu tối đa runtime
    private int damage; // Sát thương runtime
    private bool hasNotifiedDeath; // Đã thông báo chết hay chưa

    public UnitData Data => data;
    public Faction Faction => faction;
    public int MaxHealth => maxHealth;
    public int Damage => damage;
    public UnitController CurrentTarget { get; set; }
    public int LaneIndex => laneIndex;
    public bool HasLane => laneIndex >= 0;
    public bool IsDead => healthModule == null || healthModule.IsDead;

    private void Awake()
    {
        healthModule = GetComponent<HealthModule>();

        if (healthModule == null)
        {
            Debug.LogWarning($"{nameof(UnitController)} on {name} is missing HealthModule.", this);
            return;
        }

        if (data != null)
        {
            faction = data.Faction;
        }

        maxHealth = data != null ? data.MaxHealth : 1;
        damage = data != null ? data.Damage : 0;
        healthModule.Init(this, maxHealth);
    }

    private void Start()
    {
        if (data == null)
        {
            Debug.LogWarning($"{nameof(UnitController)} on {name} is missing UnitData.", this);
        }
    }

    public void Init(UnitData data)
    {
        Init(data, laneIndex);
    }

    public void Init(UnitData data, int laneIndex)
    {
        Init(data, laneIndex, 0f, 0f);
    }

    public void Init(UnitData data, int laneIndex, float healthBonusPercent, float damageBonusPercent)
    {
        this.data = data;
        this.faction = data != null ? data.Faction : default;

        if (healthModule == null)
        {
            healthModule = GetComponent<HealthModule>();
        }

        if (healthModule == null)
        {
            Debug.LogWarning($"{nameof(UnitController)} on {name} is missing HealthModule.", this);
            return;
        }

        maxHealth = CalculateStatWithBonus(this.data != null ? this.data.MaxHealth : 1, healthBonusPercent);
        damage = CalculateStatWithBonus(this.data != null ? this.data.Damage : 0, damageBonusPercent);
        healthModule.Init(this, maxHealth);

        if (laneIndex >= 0)
        {
            SetLane(laneIndex);
        }
    }

    private int CalculateStatWithBonus(int baseValue, float bonusPercent)
    {
        float multiplier = 1f + Mathf.Max(0f, bonusPercent) * 0.01f;
        return Mathf.Max(0, Mathf.RoundToInt(baseValue * multiplier));
    }

    public void SetLane(int laneIndex)
    {
        if (this.laneIndex == laneIndex)
        {
            return;
        }

        UnitLaneManager laneManager = UnitLaneManager.Instance;

        if (laneManager != null && this.laneIndex >= 0)
        {
            laneManager.Unregister(this);
        }

        this.laneIndex = laneIndex;

        if (laneManager != null)
        {
            laneManager.Register(this, laneIndex);
        }
        else
        {
            Debug.LogWarning($"{nameof(UnitController)} on {name} cannot register lane because UnitLaneManager is missing.", this);
        }
    }

    public bool IsEnemyOf(UnitController other)
    {
        if (other == null || other == this)
        {
            return false;
        }

        return faction != other.faction;
    }

    public void TakeDamage(int damage)
    {
        if (healthModule == null)
        {
            Debug.LogWarning($"{nameof(UnitController)} on {name} cannot take damage because HealthModule is missing.", this);
            return;
        }

        healthModule.TakeDamage(damage);
    }

    public void NotifyDied()
    {
        if (hasNotifiedDeath)
        {
            return;
        }

        hasNotifiedDeath = true;

        if (faction == Faction.Enemy && WaveManager.Instance != null)
        {
            WaveManager.Instance.NotifyEnemyDied(this);
        }
    }

    private void OnDestroy()
    {
        if (UnitLaneManager.Instance != null)
        {
            UnitLaneManager.Instance.Unregister(this);
        }
    }
}
