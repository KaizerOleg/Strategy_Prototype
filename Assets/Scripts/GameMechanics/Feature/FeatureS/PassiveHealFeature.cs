using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPassiveHealFeature",
    menuName = "Game/Features/Passive/Heal"
)]
public class PassiveHealFeature : PassiveEffectFeature
{
    public override List<FeatureTarget> GetPassiveTargets(
        FeatureContext context)
    {
        List<FeatureTarget> targets = new();

        if (context == null)
            return targets;

        if (context.Unit == null)
            return targets;

        targets.Add(
            new FeatureTarget(
                unit: context.Unit
            )
        );

        return targets;
    }

    protected override bool CheckRequirements(
        FeatureContext context)
    {
        if (context == null)
            return false;

        if (context.Unit == null)
            return false;

        return true;
    }
}