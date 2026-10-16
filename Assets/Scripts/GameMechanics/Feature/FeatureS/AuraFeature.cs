using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewAuraFeature",
    menuName = "Game/Features/Passive/Aura"
)]
public class AuraFeature : PassiveEffectFeature
{
    [Header("Aura")]
    [Min(1)]
    [SerializeField] private int range = 1;

    public int Range =>
        range;

    public override List<FeatureTarget> GetPassiveTargets(
        FeatureContext context)
    {
        List<FeatureTarget> targets = new();

        if (context == null)
            return targets;

        if (context.Unit == null)
            return targets;

        if (context.Player == null)
            return targets;

        if (context.Unit.CurrentTile == null)
            return targets;

        Unit source =
            context.Unit;

        List<Unit> units =
            new List<Unit>(context.Player.Units);

        foreach (Unit target in units)
        {
            if (target == null)
                continue;

            if (target == source)
                continue;

            if (target.CurrentTile == null)
                continue;

            int distance =
                source.CurrentTile.Coordinates.GetDistance(
                    target.CurrentTile.Coordinates
                );

            if (distance > 0 &&
                distance <= range)
            {
                targets.Add(
                    new FeatureTarget(
                        unit: target
                    )
                );
            }
        }

        return targets;
    }

    protected override bool CheckRequirements(
        FeatureContext context)
    {
        if (context == null)
            return false;

        if (context.Unit == null)
            return false;

        if (context.Target == null)
            return false;

        if (context.Target.Unit == null)
            return false;

        Unit source =
            context.Unit;

        Unit target =
            context.Target.Unit;

        if (source == target)
            return false;

        if (source.Owner != target.Owner)
            return false;

        if (source.CurrentTile == null ||
            target.CurrentTile == null)
        {
            return false;
        }

        int distance =
            source.CurrentTile.Coordinates.GetDistance(
                target.CurrentTile.Coordinates
            );

        return distance > 0 &&
               distance <= range;
    }
}