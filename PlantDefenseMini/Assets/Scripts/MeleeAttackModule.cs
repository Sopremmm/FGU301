public class MeleeAttackModule : AttackModuleBase
{
    protected override void ExecuteAttack(UnitController target)
    {
        if (target == null || owner == null || owner.Data == null)
        {
            return;
        }

        target.TakeDamage(owner.Damage);
    }
}
