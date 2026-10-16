public class EffectTarget
{
    public Unit Unit { get; }
    public City City { get; }
    public Building Building { get; }

    public EffectTarget(
        Unit unit = null,
        City city = null,
        Building building = null)
    {
        Unit = unit;
        City = city;
        Building = building;
    }

    public EffectContainer EffectContainer
    {
        get
        {
            if (Unit != null)
                return Unit.Effects;

            if (City != null)
                return City.Effects;

            if (Building != null)
                return Building.Effects;

            return null;
        }
    }
}