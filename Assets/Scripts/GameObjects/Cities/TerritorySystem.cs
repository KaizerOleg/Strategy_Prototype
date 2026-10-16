using System.Collections.Generic;
using UnityEngine;

public class TerritorySystem : MonoBehaviour
{
    [SerializeField] private HexGrid grid;

    public void GenerateTerritory(City city)
    {
        if (city == null)
        {
            Debug.LogError("City is null.");
            return;
        }

        if (city.Data == null)
        {
            Debug.LogError(
                "CityData is missing."
            );

            return;
        }

        if (city.CenterTile == null)
        {
            Debug.LogError(
                "City center tile is missing."
            );

            return;
        }

        if (grid == null)
        {
            Debug.LogError(
                "HexGrid is missing."
            );

            return;
        }

        List<HexTile> territory = new();

        HexTile center =
            city.CenterTile;

        int radius =
            city.Data.territoryRadius;

        foreach (HexTile tile in grid.GetAllTiles())
        {
            if (tile == null)
                continue;

            int distance =
                center.Coordinates.GetDistance(
                    tile.Coordinates
                );

            if (distance > radius)
                continue;

            // Якщо територія вже належить іншому власнику,
            // не забираємо її.
            if (tile.HasOwner() && tile.Owner != city.Owner)
                continue;

            territory.Add(tile);
        }

        city.SetTerritory(territory);
    }
}