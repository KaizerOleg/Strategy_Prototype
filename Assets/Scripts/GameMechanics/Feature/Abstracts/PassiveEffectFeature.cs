using UnityEngine;

public abstract class PassiveEffectFeature : Feature
{
    [Header("Passive Effect")]
    [SerializeField] private EffectData effect;

    public EffectData Effect => effect;

    public override bool CanApplyPassive(
        FeatureContext context)
    {
        if (!IsPassive)
            return false;

        if (context == null)
            return false;

        if (effect == null)
            return false;

        if (context.EffectSystem == null)
            return false;

        return CheckRequirements(context);
    }

    public override ActiveEffect ApplyPassive(
        FeatureContext context)
    {
        if (!CanApplyPassive(context))
            return null;

        if (context.Target == null)
            return null;

        return context.EffectSystem.ApplyPassiveEffect(
            effect,
            context,
            this
        );
    }

    protected abstract bool CheckRequirements(
        FeatureContext context);
}