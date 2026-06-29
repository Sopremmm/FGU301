using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class TargetFinderModule : MonoBehaviour
{
    private UnitController owner; // Unit sở hữu module này
    private bool warnedMissingLane; // Đã cảnh báo thiếu lane hay chưa
    private bool warnedMissingLaneManager; // Đã cảnh báo thiếu lane manager hay chưa

    private void Awake()
    {
        owner = GetComponent<UnitController>();

        if (owner == null)
        {
            Debug.LogWarning($"{nameof(TargetFinderModule)} on {name} is missing UnitController.", this);
        }
    }

    private void Update()
    {
        if (owner == null || owner.IsDead || owner.Data == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            owner.CurrentTarget = null;
            return;
        }

        if (IsValidTarget(owner.CurrentTarget))
        {
            return;
        }

        owner.CurrentTarget = FindNearestTarget();
    }

    private UnitController FindNearestTarget()
    {
        if (!owner.HasLane)
        {
            if (!warnedMissingLane)
            {
                Debug.LogWarning($"{nameof(TargetFinderModule)} on {name} cannot find target because owner has no lane.", this);
                warnedMissingLane = true;
            }

            return null;
        }

        UnitLaneManager laneManager = UnitLaneManager.Instance;

        if (laneManager == null)
        {
            if (!warnedMissingLaneManager)
            {
                Debug.LogWarning($"{nameof(TargetFinderModule)} on {name} cannot find target because UnitLaneManager is missing.", this);
                warnedMissingLaneManager = true;
            }

            return null;
        }

        IReadOnlyList<UnitController> units = laneManager.GetUnitsInLane(owner.LaneIndex);
        UnitController nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (UnitController unit in units)
        {
            if (!IsValidTarget(unit))
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, unit.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = unit;
            }
        }

        return nearestTarget;
    }

    private bool IsValidTarget(UnitController target)
    {
        if (target == null || target == owner || target.IsDead)
        {
            return false;
        }

        if (!owner.IsEnemyOf(target))
        {
            return false;
        }

        if (!owner.HasLane || !target.HasLane || owner.LaneIndex != target.LaneIndex)
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= owner.Data.AttackRange;
    }
}
