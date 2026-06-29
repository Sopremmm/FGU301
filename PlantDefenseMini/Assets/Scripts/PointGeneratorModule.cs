using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class PointGeneratorModule : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator; // Animator điều khiển animation tạo điểm
    [SerializeField] private string generateTriggerParameter = "Generate"; // Tên trigger animation tạo điểm

    private UnitController owner; // Unit sở hữu module này
    private ResourceCharacterData resourceData; // Data sinh điểm của unit
    private float cooldownTimer; // Thời gian chờ trước lần tạo điểm tiếp theo
    private bool isGenerating; // Đang chạy animation tạo điểm hay không
    private bool warnedInvalidData; // Đã cảnh báo sai data hay chưa

    private void Awake()
    {
        owner = GetComponent<UnitController>();

        if (owner == null)
        {
            Debug.LogWarning($"{nameof(PointGeneratorModule)} on {name} is missing UnitController.", this);
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            return;
        }

        if (owner == null || owner.IsDead)
        {
            return;
        }

        if (!TryGetResourceData())
        {
            return;
        }

        if (isGenerating)
        {
            return;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        BeginGenerate();
    }

    public void OnGeneratePoint()
    {
        if (!isGenerating || owner == null || owner.IsDead || !TryGetResourceData())
        {
            return;
        }

        CurrencyManager currencyManager = CurrencyManager.Instance;

        if (currencyManager == null)
        {
            Debug.LogWarning($"{nameof(PointGeneratorModule)} on {name} cannot generate points because CurrencyManager is missing.", this);
            return;
        }

        currencyManager.Add(resourceData.PointAmount);
    }

    public void OnGenerateAnimationEnd()
    {
        if (!isGenerating)
        {
            return;
        }

        isGenerating = false;
        cooldownTimer = resourceData != null ? resourceData.GenerateCooldown : 0f;
    }

    private void BeginGenerate()
    {
        isGenerating = true;

        if (animator == null)
        {
            Debug.LogWarning($"{nameof(PointGeneratorModule)} on {name} cannot play generate animation because Animator is missing.", this);
            OnGenerateAnimationEnd();
            return;
        }

        if (string.IsNullOrEmpty(generateTriggerParameter))
        {
            Debug.LogWarning($"{nameof(PointGeneratorModule)} on {name} cannot play generate animation because trigger parameter is empty.", this);
            OnGenerateAnimationEnd();
            return;
        }

        animator.SetTrigger(generateTriggerParameter);
    }

    private bool TryGetResourceData()
    {
        if (resourceData != null)
        {
            return true;
        }

        resourceData = owner != null ? owner.Data as ResourceCharacterData : null;

        if (resourceData == null && !warnedInvalidData)
        {
            Debug.LogWarning($"{nameof(PointGeneratorModule)} on {name} needs ResourceCharacterData on UnitController.", this);
            warnedInvalidData = true;
        }

        return resourceData != null;
    }
}
