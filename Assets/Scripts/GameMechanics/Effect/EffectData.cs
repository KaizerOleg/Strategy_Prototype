using UnityEngine;

public abstract class EffectData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string effectName;

    [TextArea]
    [SerializeField] private string description;

    [Header("Duration")]
    [SerializeField]
    private EffectDurationType durationType =
        EffectDurationType.Turns;

    [Min(1)]
    [SerializeField]
    private int defaultDuration = 1;

    [Header("Stacking")]
    [SerializeField]
    private EffectStackMode stackMode =
        EffectStackMode.RefreshDuration;

    public string EffectName => effectName;
    public string Description => description;

    public EffectDurationType DurationType =>
        durationType;

    public int DefaultDuration =>
        defaultDuration;

    public EffectStackMode StackMode =>
        stackMode;

    public virtual void OnApplied(
        ActiveEffect effect)
    {
    }

    public virtual void OnTurnStarted(
        ActiveEffect effect)
    {
    }

    public virtual void OnTurnEnded(
        ActiveEffect effect)
    {
    }

    public virtual void OnRemoved(
        ActiveEffect effect)
    {
    }
}