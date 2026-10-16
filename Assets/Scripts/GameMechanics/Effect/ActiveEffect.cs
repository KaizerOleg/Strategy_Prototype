public class ActiveEffect
{
    public EffectData Data { get; }

    public Feature SourceFeature { get; }

    public Unit SourceUnit { get; }
    public City SourceCity { get; }
    public Building SourceBuilding { get; }

    public EffectTarget Target { get; }

    public Player SourcePlayer { get; }

    public bool IsPassive { get; }

    public int RemainingTurns { get; private set; }

    public int StackCount { get; private set; }

    public bool IsPermanent =>
        Data != null &&
        Data.DurationType ==
        EffectDurationType.Permanent;

    public bool IsTurnBased =>
        Data != null &&
        Data.DurationType ==
        EffectDurationType.Turns;

    public ActiveEffect(
        EffectData data,
        int duration,
        EffectContext context,
        Feature sourceFeature = null,
        bool isPassive = false)
    {
        Data = data;

        SourceFeature = sourceFeature;

        IsPassive = isPassive;

        RemainingTurns = duration;

        StackCount = 1;

        if (context != null)
        {
            SourceUnit =
                context.SourceUnit;

            SourceCity =
                context.SourceCity;

            SourceBuilding =
                context.SourceBuilding;

            Target =
                context.Target;

            SourcePlayer =
                context.SourcePlayer;
        }
    }

    public void AddStack()
    {
        StackCount++;
    }

    public void SetStacks(int amount)
    {
        StackCount =
            amount < 1
                ? 1
                : amount;
    }

    public void RefreshDuration(
        int duration)
    {
        RemainingTurns = duration;
    }

    public void DecreaseDuration()
    {
        if (!IsTurnBased)
            return;

        if (RemainingTurns > 0)
            RemainingTurns--;
    }

    public bool HasExpired()
    {
        if (!IsTurnBased)
            return false;

        return RemainingTurns <= 0;
    }
}