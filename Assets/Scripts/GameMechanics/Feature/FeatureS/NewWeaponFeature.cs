using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWeaponsFeature",
    menuName = "Game/Features/Passive/New Weapons"
)]
public class NewWeaponsFeature : PassiveEffectFeature
{
    [Header("Technology")]
    [SerializeField]
    private TechnologyData requiredTechnology;

    public override List<FeatureTarget> GetPassiveTargets(
        FeatureContext context)
    {
        List<FeatureTarget> targets = new();

        if (context == null)
        {
            Debug.Log("a1");
            return targets;
        }

        if (context.Unit == null)
        {
            Debug.Log("a2");
            return targets;
        }

        targets.Add(
            new FeatureTarget(
                unit: context.Unit
            )
        );

        foreach(FeatureTarget t in targets)
        {
            Debug.Log(t.Unit.name);
        }

        return targets;
    }

    protected override bool CheckRequirements(
        FeatureContext context)
    {
        if (context == null)
        {
            return false;
        }

        if (context.Unit == null)
        {
            return false;
        }

        if (context.Player == null)
        {
            return false;
        }

        if (requiredTechnology == null)
        {
            return true;
        }

        return context.Player.HasTechnology(
            requiredTechnology
        );
    }
}