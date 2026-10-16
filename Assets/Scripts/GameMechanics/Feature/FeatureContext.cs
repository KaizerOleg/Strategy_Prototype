public class FeatureContext
{
    public Unit Unit { get; }
    public City City { get; }
    public Building Building { get; }

    public Player Player { get; }

    public FeatureTarget Target { get; }

    public EffectSystem EffectSystem { get; }

    public FeatureContext(
        Unit unit = null,
        City city = null,
        Building building = null,
        Player player = null,
        FeatureTarget target = null,
        EffectSystem effectSystem = null)
    {
        Unit = unit;
        City = city;
        Building = building;

        Player = player;

        Target = target;

        EffectSystem = effectSystem;
    }
}