
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkBuilding : NetworkBehaviour
{
    public NetworkVariable<int> OwnerPlayerId =
        new(-1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> BuildingTypeId =
        new(-1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> TileQ =
        new(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> TileR =
        new(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<ulong> CityNetworkObjectId =
        new(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private Building building;
    private HexGrid grid;
    private PlayerManager playerManager;
    private Coroutine initializationCoroutine;

    private void Awake()
    {
        building = GetComponent<Building>();
        FindReferences();
    }

    private void FindReferences()
    {
        if (grid == null)
            grid = FindFirstObjectByType<HexGrid>();

        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        OwnerPlayerId.OnValueChanged += OnNetworkDataChanged;
        BuildingTypeId.OnValueChanged += OnNetworkDataChanged;
        TileQ.OnValueChanged += OnNetworkDataChanged;
        TileR.OnValueChanged += OnNetworkDataChanged;
        CityNetworkObjectId.OnValueChanged += OnCityIdChanged;

        FindReferences();

        if (!IsServer)
            StartInitialization();
    }

    public override void OnNetworkDespawn()
    {
        OwnerPlayerId.OnValueChanged -= OnNetworkDataChanged;
        BuildingTypeId.OnValueChanged -= OnNetworkDataChanged;
        TileQ.OnValueChanged -= OnNetworkDataChanged;
        TileR.OnValueChanged -= OnNetworkDataChanged;
        CityNetworkObjectId.OnValueChanged -= OnCityIdChanged;

        if (initializationCoroutine != null)
        {
            StopCoroutine(initializationCoroutine);
            initializationCoroutine = null;
        }

        base.OnNetworkDespawn();
    }

    public void InitializeNetwork(
        Player owner,
        BuildingData data,
        City city,
        HexTile tile,
        int buildingTypeId)
    {
        if (!IsServer || owner == null ||
            data == null || city == null || tile == null)
        {
            return;
        }

        NetworkObject cityNetworkObject =
            city.GetComponent<NetworkObject>();

        if (cityNetworkObject == null ||
            !cityNetworkObject.IsSpawned)
        {
            Debug.LogError(
                "[NetworkBuilding] City must have a spawned NetworkObject.",
                this);
            return;
        }

        OwnerPlayerId.Value = owner.PlayerId;
        BuildingTypeId.Value = buildingTypeId;
        TileQ.Value = tile.Coordinates.Q;
        TileR.Value = tile.Coordinates.R;
        CityNetworkObjectId.Value =
            cityNetworkObject.NetworkObjectId;
    }

    private void OnNetworkDataChanged(int previous, int current)
    {
        if (!IsServer)
            StartInitialization();
    }

    private void OnCityIdChanged(ulong previous, ulong current)
    {
        if (!IsServer)
            StartInitialization();
    }

    private void StartInitialization()
    {
        if (IsServer || !IsSpawned ||
            initializationCoroutine != null)
        {
            return;
        }

        initializationCoroutine =
            StartCoroutine(InitializeRoutine());
    }

    private IEnumerator InitializeRoutine()
    {
        while (IsSpawned && !IsServer)
        {
            if (TryInitializeClientBuilding())
            {
                initializationCoroutine = null;
                yield break;
            }

            yield return null;
        }

        initializationCoroutine = null;
    }

    private bool TryInitializeClientBuilding()
    {
        if (building == null)
            return false;

        FindReferences();

        if (grid == null || playerManager == null)
            return false;

        if (OwnerPlayerId.Value < 0 ||
            BuildingTypeId.Value < 0 ||
            CityNetworkObjectId.Value == 0)
        {
            return false;
        }

        Player owner =
            playerManager.GetPlayer(OwnerPlayerId.Value);

        if (owner == null)
            return false;

        CivilizationData civilization = owner.GetCivilization();

        if (civilization == null ||
            civilization.buildingDatabase == null)
        {
            return false;
        }

        var buildings = civilization.buildingDatabase.Buildings;

        if (BuildingTypeId.Value >= buildings.Count)
        {
            Debug.LogError(
                $"[NetworkBuilding] Invalid BuildingTypeId: " +
                $"{BuildingTypeId.Value}.",
                this);
            return false;
        }

        BuildingData buildingData = buildings[BuildingTypeId.Value];

        if (buildingData == null)
            return false;

        HexTile tile = grid.GetTile(
            new HexCoordinates(TileQ.Value, TileR.Value));

        if (tile == null)
            return false;

        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                CityNetworkObjectId.Value,
                out NetworkObject cityNetworkObject))
        {
            return false;
        }

        City city = cityNetworkObject.GetComponent<City>();

        if (city == null || city.Data == null)
            return false;

        if (building.Data == null)
        {
            building.Initialize(buildingData, city, tile);
        }

        return building.Data == buildingData &&
               building.City == city &&
               building.Tile == tile;
    }
}
