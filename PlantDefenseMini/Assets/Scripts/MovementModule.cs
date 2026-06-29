using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class MovementModule : MonoBehaviour
{
    private enum FacingDirection
    {
        Left,
        Right
    }

    [Header("Facing")]
    [SerializeField] private FacingDirection initialFacingDirection = FacingDirection.Left; // Hướng nhìn ban đầu

    [Header("Animation")]
    [SerializeField] private Animator animator; // Animator điều khiển animation di chuyển
    [SerializeField] private string walkBoolParameter = "IsWalking"; // Tên bool animation walk

    private UnitController owner; // Unit sở hữu module này
    private bool isFacingRight; // Đang nhìn sang phải hay không
    private bool isWalking; // Đang ở trạng thái đi bộ hay không

    private void Awake()
    {
        owner = GetComponent<UnitController>();

        if (owner == null)
        {
            Debug.LogWarning($"{nameof(MovementModule)} on {name} is missing UnitController.", this);
        }

        isFacingRight = initialFacingDirection == FacingDirection.Right;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            SetWalking(false);
            return;
        }

        if (owner == null || owner.IsDead || owner.Data == null)
        {
            SetWalking(false);
            return;
        }

        if (owner.Data.MovementType == MovementType.None)
        {
            SetWalking(false);
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        UnitController target = owner.CurrentTarget;

        if (target == null || target.IsDead)
        {
            MoveByMovementType();
            return;
        }

        UpdateFacingByTarget(target);

        if (IsTargetInAttackRange(target))
        {
            SetWalking(false);
            return;
        }

        if (owner.Data.MovementType == MovementType.MoveLeft && IsTargetOnLeft(target))
        {
            MoveLeft();
            return;
        }

        SetWalking(false);
    }

    private void MoveByMovementType()
    {
        if (owner.Data.MovementType == MovementType.MoveLeft)
        {
            MoveLeft();
            return;
        }

        SetWalking(false);
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void MoveLeft()
    {
        if (isFacingRight)
        {
            Flip();
        }

        transform.position += Vector3.left * owner.Data.MoveSpeed * Time.deltaTime;
        SetWalking(true);

        if (GameManager.Instance != null && GameManager.Instance.HasReachedLosePosition(owner))
        {
            GameManager.Instance.Lose();
        }
    }

    private void UpdateFacingByTarget(UnitController target)
    {
        bool targetIsRight = target.transform.position.x > transform.position.x;

        if (targetIsRight != isFacingRight)
        {
            Flip();
        }
    }

    private bool IsTargetInAttackRange(UnitController target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= owner.Data.AttackRange;
    }

    private bool IsTargetOnLeft(UnitController target)
    {
        return target.transform.position.x < transform.position.x;
    }

    private void SetWalking(bool value)
    {
        if (isWalking == value)
        {
            return;
        }

        isWalking = value;

        if (animator != null && !string.IsNullOrEmpty(walkBoolParameter))
        {
            animator.SetBool(walkBoolParameter, isWalking);
        }
    }
}
