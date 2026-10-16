using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFireAttackFeature",
    menuName = "Game/Features/Fire Attack"
)]
public class FireAttackFeature : Feature
{
    [Header("Attack")]
    [Min(1)]
    [SerializeField] public int attackRange = 1;

    [Min(1)]
    [SerializeField] private int damage = 20;

    [Header("Burn")]
    [SerializeField] private BurnEffect burnEffect;

    [Min(1)]
    [SerializeField] private int burnDuration = 3;

    public override bool RequiresTarget => true;

    public override bool CanActivate(
        FeatureContext context)
    {
        if (context == null)
            return false;

        if (context.Unit == null)
            return false;

        return true;
    }

    public override void Activate(
        FeatureContext context)
    {
        if (!CanActivate(context))
            return;

        if (context.Target == null)
            return;

        if (context.Target.Unit == null)
            return;

        Unit attacker =
            context.Unit;

        Unit target =
            context.Target.Unit;

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

        if(distance > attackRange)
        {
            return;
        }

        target.TakeDamage(damage);

        if (burnEffect == null)
            return;

        if (context.EffectSystem == null)
        {
            Debug.LogError(
                "EffectSystem is missing in FeatureContext."
            );

            return;
        }

        EffectContext effectContext =
            new EffectContext(
                target: new EffectTarget(
                    unit: target
                ),
                sourceUnit: attacker,
                sourcePlayer: attacker.Owner
            );

        context.EffectSystem.ApplyEffect(
            burnEffect,
            burnDuration,
            effectContext
        );
    }
}