public class EffectContext
{
    public Unit SourceUnit { get; }
    public City SourceCity { get; }
    public Building SourceBuilding { get; }

    public EffectTarget Target { get; }

    public Player SourcePlayer { get; }

    public EffectContext(
        EffectTarget target,
        Unit sourceUnit = null,
        City sourceCity = null,
        Building sourceBuilding = null,
        Player sourcePlayer = null)
    {
        Target = target;

        SourceUnit = sourceUnit;
        SourceCity = sourceCity;
        SourceBuilding = sourceBuilding;

        SourcePlayer = sourcePlayer;
    }
}