public class FeatureTarget
{
    public Unit Unit { get; }
    public City City { get; }
    public Building Building { get; }
    public HexTile Tile { get; }

    public FeatureTarget(
        Unit unit = null,
        City city = null,
        Building building = null,
        HexTile tile = null)
    {
        Unit = unit;
        City = city;
        Building = building;
        Tile = tile;
    }
}