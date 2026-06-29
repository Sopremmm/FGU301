using UnityEngine;
using UnityEngine.UI;

public class HealthModule : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image healthBarImage; // Image fill của thanh máu

    private const string HealthBarPath = "UI/Health/HealthBar"; // Đường dẫn tự tìm thanh máu

    private UnitController owner; // Unit sở hữu module này
    private int maxHealth; // Máu tối đa
    private int currentHealth; // Máu hiện tại
    private bool isInitialized; // Đã khởi tạo hay chưa
    private bool isDead; // Đã chết hay chưa

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        FindHealthBarIfNeeded();
    }

    public void Init(UnitController owner, int maxHealth)
    {
        FindHealthBarIfNeeded();

        this.owner = owner;
        this.maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = this.maxHealth;
        isDead = false;
        isInitialized = true;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (!isInitialized || isDead || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        owner?.NotifyDied();

        GameObject target = owner != null ? owner.gameObject : gameObject;
        Destroy(target);
    }

    private void UpdateHealthBar()
    {
        FindHealthBarIfNeeded();

        if (healthBarImage == null)
        {
            return;
        }

        healthBarImage.fillAmount = maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;
    }

    private void FindHealthBarIfNeeded()
    {
        if (healthBarImage != null)
        {
            return;
        }

        Transform healthBarTransform = transform.Find(HealthBarPath);

        if (healthBarTransform != null)
        {
            healthBarImage = healthBarTransform.GetComponent<Image>();
        }
    }
}
