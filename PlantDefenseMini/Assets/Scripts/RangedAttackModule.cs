using UnityEngine;

public class RangedAttackModule : AttackModuleBase
{
    [SerializeField] private Transform shootPoint; // Vị trí spawn projectile

    protected override void ExecuteAttack(UnitController target)
    {
        if (target == null || owner == null || owner.Data == null)
        {
            return;
        }

        if (owner.Data.ProjectilePrefab == null)
        {
            Debug.LogWarning($"{nameof(RangedAttackModule)} on {name} is missing ProjectilePrefab in UnitData.", this);
            return;
        }

        if (shootPoint == null)
        {
            Debug.LogWarning($"{nameof(RangedAttackModule)} on {name} is missing shootPoint.", this);
            return;
        }

        Projectile projectile = Instantiate(
            owner.Data.ProjectilePrefab,
            shootPoint.position,
            shootPoint.rotation
        );

        projectile.Init(target, owner.Damage);
    }
}
