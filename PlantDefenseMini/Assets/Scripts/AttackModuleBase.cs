using UnityEngine;

[RequireComponent(typeof(UnitController))]
public abstract class AttackModuleBase : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator; // Animator điều khiển animation tấn công
    [SerializeField] private string attackTriggerParameter = "Attack"; // Tên trigger animation attack
    [SerializeField] private bool requireTargetInRangeOnHit = true; // Có cần target còn trong tầm khi hit không

    protected UnitController owner; // Unit sở hữu module này

    private float cooldownTimer; // Thời gian chờ trước lần đánh tiếp theo
    private bool isAttacking; // Đang chạy animation attack hay không
    private UnitController attackTarget; // Target được khóa cho lần đánh hiện tại

    protected virtual void Awake()
    {
        owner = GetComponent<UnitController>();

        if (owner == null)
        {
            Debug.LogWarning($"{GetType().Name} on {name} is missing UnitController.", this);
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

        if (owner == null || owner.IsDead || owner.Data == null)
        {
            return;
        }

        if (isAttacking)
        {
            return;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        UnitController target = owner.CurrentTarget;

        if (!IsValidAttackTarget(target, true))
        {
            return;
        }

        BeginAttack(target);
    }

    private void BeginAttack(UnitController target)
    {
        attackTarget = target;
        isAttacking = true;

        if (animator == null)
        {
            Debug.LogWarning($"{GetType().Name} on {name} cannot play attack because Animator is missing.", this);
            OnAttackAnimationEnd();
            return;
        }

        if (string.IsNullOrEmpty(attackTriggerParameter))
        {
            Debug.LogWarning($"{GetType().Name} on {name} cannot play attack because attack trigger parameter is empty.", this);
            OnAttackAnimationEnd();
            return;
        }

        animator.SetTrigger(attackTriggerParameter);
    }

    public void OnAttackHit()
    {
        if (!isAttacking)
        {
            return;
        }

        if (!IsValidAttackTarget(attackTarget, requireTargetInRangeOnHit))
        {
            return;
        }

        ExecuteAttack(attackTarget);
    }

    public void OnAttackAnimationEnd()
    {
        if (!isAttacking)
        {
            return;
        }

        isAttacking = false;
        attackTarget = null;
        cooldownTimer = owner != null && owner.Data != null ? owner.Data.AttackCooldown : 0f;
    }

    private bool IsValidAttackTarget(UnitController target, bool requireInRange)
    {
        if (target == null || target.IsDead)
        {
            return false;
        }

        if (owner == null || owner.IsDead || owner.Data == null)
        {
            return false;
        }

        if (!owner.IsEnemyOf(target))
        {
            return false;
        }

        if (!requireInRange)
        {
            return true;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= owner.Data.AttackRange;
    }

    protected abstract void ExecuteAttack(UnitController target);
}
