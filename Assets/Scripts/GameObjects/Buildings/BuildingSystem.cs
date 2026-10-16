
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private TechnologySystem technologySystem;
    [SerializeField] private TurnManager turnManager;

    public IReadOnlyList<BuildingData> GetAvailableBuildings(City city)
    {
        if (city == null || city.Owner == null)
            return new List<BuildingData>();

        CivilizationData civilization =
            city.Owner.GetCivilization();

        if (civilization == null ||
            civilization.buildingDatabase == null)
        {
            Debug.LogError("BuildingDatabase is missing.");
            return new List<BuildingData>();
        }

        return civilization.buildingDatabase.Buildings;
    }

    public int GetBuildingTypeId(Player player, BuildingData data)
    {
        if (player == null || data == null)
            return -1;

        CivilizationData civilization = player.GetCivilization();

        if (civilization == null ||
            civilization.buildingDatabase == null)
        {
            return -1;
        }

        var buildings = civilization.buildingDatabase.Buildings;

        for (int i = 0; i < buildings.Count; i++)
        {
            if (buildings[i] == data)
                return i;
        }

        return -1;
    }

    public bool CanBuild(
        Player player,
        City city,
        HexTile tile,
        BuildingData building)
    {
        if (player == null || city == null ||
            tile == null || building == null)
        {
            return false;
        }

        if (city.Owner != player)
            return false;

        if (!HasRequiredTechnology(player, building))
            return false;

        if (!city.ControlsTile(tile) ||
            !city.CanBuildThisTurn())
        {
            return false;
        }

        if (tile.HasBuilding() ||
            tile.HasCity() ||
            !tile.HasOwner())
        {
            return false;
        }

        if (!CanBuildOnTerrain(building, tile.Terrain))
            return false;

        return CanAffordBuilding(player, building, city);
    }

    private bool HasRequiredTechnology(
        Player player,
        BuildingData building)
    {
        if (player == null || building == null)
            return false;

        if (technologySystem == null)
        {
            Debug.LogError("TechnologySystem is missing.");
            return false;
        }

        return technologySystem.HasTechnology(
            player,
            building.technologyRequirement);
    }

    private bool CanBuildOnTerrain(
        BuildingData building,
        TerrainData terrain)
    {
        if (building == null || terrain == null)
            return false;

        if (building.canBuildOnAnyTerrain)
            return true;

        if (building.allowedTerrains == null)
            return false;

        foreach (TerrainData allowedTerrain in building.allowedTerrains)
        {
            if (allowedTerrain == terrain)
                return true;
        }

        return false;
    }

    public bool CanAffordBuilding(
        Player player,
        BuildingData building,
        City city)
    {
        if (player == null ||
            player.Resources == null ||
            building == null ||
            city == null)
        {
            return false;
        }

        if (city.Owner != player || !city.CanBuildThisTurn())
            return false;

        if (!HasRequiredTechnology(player, building))
            return false;

        return player.Resources.CanAfford(
            building.productionCost,
            building.goldCost);
    }

    public bool Build(
        Player player,
        City city,
        HexTile tile,
        BuildingData buildingData)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        bool isNetworkGame =
            networkManager != null && networkManager.IsListening;

        // У мережевій грі створювати будівлі може тільки сервер.
        if (isNetworkGame && !networkManager.IsServer)
            return false;

        if (!CanBuild(player, city, tile, buildingData))
            return false;

        if (buildingPrefab == null)
        {
            Debug.LogError("Building prefab is missing.");
            return false;
        }

        int buildingTypeId = GetBuildingTypeId(player, buildingData);

        if (buildingTypeId < 0)
        {
            Debug.LogError(
                "BuildingData was not found in the owner's BuildingDatabase.");
            return false;
        }

        NetworkObject networkObject = null;
        NetworkBuilding networkBuilding = null;

        if (isNetworkGame)
        {
            networkObject =
                buildingPrefab.GetComponent<NetworkObject>();

            networkBuilding =
                buildingPrefab.GetComponent<NetworkBuilding>();

            NetworkObject cityNetworkObject =
                city.GetComponent<NetworkObject>();

            if (networkObject == null ||
                networkBuilding == null ||
                cityNetworkObject == null ||
                !cityNetworkObject.IsSpawned)
            {
                Debug.LogError(
                    "Network building prefab requires NetworkObject and " +
                    "NetworkBuilding. The city must also be network-spawned.");
                return false;
            }
        }

        // Ресурси списуються лише після перевірок.
        if (!player.Resources.TrySpend(
                buildingData.productionCost,
                buildingData.goldCost))
        {
            return false;
        }

        GameObject buildingObject = Instantiate(
            buildingPrefab,
            tile.transform.position,
            Quaternion.identity);

        Building building = buildingObject.GetComponent<Building>();

        if (building == null)
        {
            Debug.LogError("Building component is missing.");
            Destroy(buildingObject);

            player.Resources.Add(
                new Yield(
                    0,
                    buildingData.productionCost,
                    buildingData.goldCost,
                    0));

            return false;
        }

        building.Initialize(buildingData, city, tile);

        if (isNetworkGame)
        {
            // Беремо компоненти саме зі створеного екземпляра.
            networkObject = buildingObject.GetComponent<NetworkObject>();
            networkBuilding = buildingObject.GetComponent<NetworkBuilding>();

            if (networkObject == null || networkBuilding == null)
            {
                Debug.LogError("Network components are missing on the building instance.");
                Destroy(buildingObject);

                // Повертаємо витрачені ресурси.
                player.Resources.Add(
                    new Yield(
                        0,
                        buildingData.productionCost,
                        buildingData.goldCost,
                        0));

                return false;
            }

            networkObject.Spawn();

            networkBuilding.InitializeNetwork(
                player,
                buildingData,
                city,
                tile,
                buildingTypeId);

        }

        city.RegisterBuildingBuilt();
        return true;
    }
}
