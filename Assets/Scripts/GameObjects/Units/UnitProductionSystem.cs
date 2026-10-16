using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitProductionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TechnologySystem technologySystem;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private HexGrid grid;

    public IReadOnlyList<UnitData> GetAvailableUnits(City city)
    {
        if (city == null || turnManager == null)
            return new List<UnitData>();

        Player player = city.Owner;

        if (player == null)
            return new List<UnitData>();

        CivilizationData civilization = player.GetCivilization();

        if (civilization == null ||
            civilization.unitDatabase == null)
        {
            Debug.LogError("UnitDatabase is missing.");
            return new List<UnitData>();
        }

        return civilization.unitDatabase.Units;
    }

    public bool CanProduce(
        Player player,
        City city,
        UnitData unitData)
    {
        if (player == null ||
            city == null ||
            unitData == null)
        {
            return false;
        }

        if (city.Owner != player)
            return false;

        if (turnManager == null ||
            turnManager.CurrentPlayer != player)
        {
            return false;
        }

        if (!city.CanAffordProduction(unitData.productionCost))
            return false;

        if (!HasRequiredTechnology(player, unitData))
            return false;

        if (city.CenterTile == null ||
            city.CenterTile.IsOccupied())
        {
            return false;
        }

        return true;
    }

    private bool HasRequiredTechnology(
        Player player,
        UnitData unitData)
    {
        if (player == null || unitData == null)
            return false;

        if (technologySystem == null)
        {
            Debug.LogError("TechnologySystem is missing.");
            return false;
        }

        return technologySystem.HasTechnology(
            player,
            unitData.technologyRequirement);
    }

    public bool ProduceOnServer(
        Player player,
        City city,
        UnitData unitData)
    {
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null ||
            !networkManager.IsListening ||
            !networkManager.IsServer)
        {
            Debug.LogError(
                "ProduceOnServer can only run on the network server.");
            return false;
        }

        if (!CanProduce(player, city, unitData))
            return false;

        if (unitPrefab == null)
        {
            Debug.LogError("Unit prefab is missing.");
            return false;
        }

        if (grid == null)
        {
            Debug.LogError("HexGrid is missing.");
            return false;
        }

        HexTile spawnTile = city.CenterTile;

        NetworkObject networkObjectPrefab =
            unitPrefab.GetComponent<NetworkObject>();

        NetworkUnit networkUnitPrefab =
            unitPrefab.GetComponent<NetworkUnit>();

        if (networkObjectPrefab == null ||
            networkUnitPrefab == null)
        {
            Debug.LogError(
                "Unit prefab must have NetworkObject and NetworkUnit.");
            return false;
        }

        // Повторна перевірка перед списанням.
        if (!CanProduce(player, city, unitData))
            return false;

        Unit unit = Instantiate(
            unitPrefab,
            spawnTile.transform.position,
            Quaternion.identity);

        NetworkObject networkObject =
            unit.GetComponent<NetworkObject>();

        NetworkUnit networkUnit =
            unit.GetComponent<NetworkUnit>();

        if (networkObject == null || networkUnit == null)
        {
            Destroy(unit.gameObject);
            Debug.LogError(
                "Created unit is missing NetworkObject or NetworkUnit.");
            return false;
        }

        // Ініціалізуємо серверну модель юніта.
        unit.Initialize(
            player,
            unitData,
            spawnTile,
            grid);

        // Списуємо виробництво лише на сервері.
        if (!city.TrySpendProduction(unitData.productionCost))
        {
            Destroy(unit.gameObject);
            return false;
        }

        // Реєструємо юніта в мережі.
        networkObject.Spawn();
        networkUnit.InitializeNetwork();

        return true;
    }
}