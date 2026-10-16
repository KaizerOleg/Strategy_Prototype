using UnityEngine;
using Unity.Netcode;

public class EconomySystem : MonoBehaviour
{
    public Yield CalculateCityYield(City city)
    {
        if (city == null)
            return new Yield();

        Yield total =
            city.Data.baseYield;

        foreach (HexTile tile in city.Territory)
        {
            if (tile == null ||
                tile.Terrain == null)
            {
                continue;
            }

            TerrainData terrain =
                tile.Terrain;

            total += new Yield(
                terrain.food,
                terrain.production,
                terrain.gold,
                terrain.science
            );

            if (tile.Building != null)
            {
                total +=
                    tile.Building.GetYield();
            }
        }

        return total;
    }

    public Yield CalculatePlayerYield(
        Player player)
    {
        if (player == null)
            return new Yield();

        Yield total = new Yield();

        foreach (City city in player.Cities)
        {
            if (city == null)
                continue;

            total +=
                CalculateCityYield(city);
        }

        return total;
    }

    public void CollectIncome(Player player)
    {
        if (player == null)
            return;

        NetworkManager networkManager = NetworkManager.Singleton;

        // У мережевій грі дохід нараховує лише сервер.
        if (networkManager != null &&
            networkManager.IsListening &&
            !networkManager.IsServer)
        {
            return;
        }

        Yield income = CalculatePlayerYield(player);

        player.Resources.Add(income);
    }
}