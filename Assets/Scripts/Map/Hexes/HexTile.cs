using UnityEngine;

public class HexTile : MonoBehaviour
{
    public HexCoordinates Coordinates { get; private set; }

    public TerrainData Terrain { get; private set; }

    public Unit OccupyingUnit { get; private set; }

    public float Height { get; private set; }

    public float Moisture { get; private set; }

    public City City { get; private set; }

    public Building Building { get; private set; }

    public Player Owner { get; private set; }

    public HexTileView View { get; private set; }

    public bool HasTerrain
    {
        get
        {
            return Terrain != null;
        }
    }

    private void Awake()
    {
        View =
            GetComponent<HexTileView>();

        if (View == null)
        {
            Debug.LogError(
                "HexTileView is missing on HexTile."
            );
        }
    }

    public bool TrySetTerrain(
    TerrainData terrain)
    {
        if (terrain == null)
            return false;

        if (Terrain != null)
            return false;

        Terrain = terrain;

        if (View != null)
        {
            View.SetTerrain(terrain);
        }

        return true;
    }

    public void SetHeight(float value)
    {
        Height = value;
    }

    public void SetMoisture(float value)
    {
        Moisture = value;
    }

    public bool HasCity()
    {
        return City != null;
    }

    public bool HasOwner()
    {
        return Owner != null;
    }

    public bool HasBuilding()
    {
        return Building != null;
    }

    public void SetCity(City city)
    {
        City = city;
    }

    public void SetBuilding(Building building)
    {
        Building = building;
    }

    public void SetOwner(Player player)
    {
        Owner = player;
    }

    public void Initialize(
        HexCoordinates coordinates)
    {
        Coordinates = coordinates;
    }

    public void SetTerrain(
        TerrainData terrain)
    {
        if (terrain == null)
        {
            Debug.LogError(
                "TerrainData is null."
            );

            return;
        }

        Terrain = terrain;

        if (View != null)
        {
            View.SetTerrain(
                terrain
            );
        }
    }

    public bool IsOccupied()
    {
        return OccupyingUnit != null;
    }

    public void SetOccupyingUnit(
        Unit unit)
    {
        OccupyingUnit = unit;
    }

    public void ClearOccupyingUnit()
    {
        OccupyingUnit = null;
    }
}