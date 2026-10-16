using UnityEngine;

[CreateAssetMenu(
    fileName = "NewAttackFeature",
    menuName = "Game/Features/Attack"
)]
public class AttackFeature : Feature
{
    [SerializeField] public int attackRange = 1;
    public override bool RequiresTarget => true;
    public override bool CanActivate(
        FeatureContext context)
    {
        if (context == null)
            return false;

        if (context.Unit == null)
            return false;

        if (context.Unit.Data == null)
            return false;

        return true;

    }

    public override void Activate(
    FeatureContext context)
    {
        if (!CanActivate(context))
            return;

        if (context.Target.Unit == null)
            return;

        if (context.Unit.Owner == context.Target.Unit)
            return;


        Unit attacker = context.Unit;
        Unit target = context.Target.Unit;

        if (attacker == target)
            return;

        if (attacker.Owner == target.Owner)
            return;

        if (attacker.CurrentTile == null ||
            target.CurrentTile == null)
        {
            return;
        }

        int distance =
            attacker.CurrentTile.Coordinates
                .GetDistance(
                    target.CurrentTile.Coordinates
                );
        if(distance>attackRange)
        {
            Debug.Log("Not in range");
            return;
        }

        target.TakeDamage(
            context.Unit.GetAttack()
        );
    }
}