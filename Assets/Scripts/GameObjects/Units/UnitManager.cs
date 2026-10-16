using Unity.Netcode;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private HexGrid grid;
    [SerializeField] private PlayerManager playerManager;

    [Header("Starting Unit")]
    [SerializeField] private int startingUnitId;

    [Header("Starting Positions")]
    [SerializeField]
    private HexCoordinates player0Start =
        new HexCoordinates(0, 0);

    [SerializeField]
    private HexCoordinates player1Start =
        new HexCoordinates(9, 9);

    public void SpawnStartingUnits()
    {
        if (NetworkManager.Singleton != null &&
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (unitPrefab == null)
        {
            Debug.LogError(
                "Unit Prefab is missing."
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

        if (playerManager == null)
        {
            Debug.LogError(
                "PlayerManager is missing."
            );

            return;
        }

        SpawnPlayerUnit(
            0,
            player0Start
        );

        SpawnPlayerUnit(
            1,
            player1Start
        );
    }

    private void SpawnPlayerUnit(
        int playerId,
        HexCoordinates coordinates)
    {
        Player player =
            playerManager.GetPlayer(playerId);

        if (player == null)
        {
            Debug.LogError(
                $"Player {playerId} was not found."
            );

            return;
        }

        CivilizationData civilization =
            player.GetCivilization();

        if (civilization == null)
        {
            Debug.LogError(
                $"Civilization for Player " +
                $"{playerId} is missing."
            );

            return;
        }

        if (civilization.unitDatabase == null)
        {
            Debug.LogError(
                $"UnitDatabase for Player " +
                $"{playerId} is missing."
            );

            return;
        }

        UnitData startingUnit =
            civilization.unitDatabase.GetUnit(
                startingUnitId
            );

        if (startingUnit == null)
        {
            Debug.LogError(
                $"Starting Unit {startingUnitId} " +
                $"was not found in civilization " +
                $"{civilization.civilizationName}."
            );

            return;
        }

        HexTile tile =
            grid.GetTile(coordinates);

        if (tile == null)
        {
            Debug.LogError(
                $"Starting tile {coordinates} " +
                $"for Player {playerId} was not found."
            );

            return;
        }

        if (tile.IsOccupied())
        {
            Debug.LogError(
                $"Starting tile {coordinates} " +
                $"is already occupied."
            );

            return;
        }

        Unit unit =
            Instantiate(
                unitPrefab,
                tile.transform.position,
                Quaternion.identity
            );

        unit.Initialize(
            player,
            startingUnit,
            tile,
            grid
        );

        NetworkObject networkObject =
            unit.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError(
                "NetworkObject is missing on Unit prefab.",
                unit
            );

            Destroy(unit.gameObject);
            return;
        }

        NetworkUnit networkUnit =
            unit.GetComponent<NetworkUnit>();

        if (networkUnit == null)
        {
            Debug.LogError(
                "NetworkUnit is missing on Unit prefab.",
                unit
            );

            Destroy(unit.gameObject);
            return;
        }

        networkObject.Spawn();

        networkUnit.InitializeNetwork();
    }
}