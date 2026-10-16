
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkCity : NetworkBehaviour
{
    public NetworkVariable<int> OwnerPlayerId =
        new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> CityTypeId =
        new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> TileQ =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> TileR =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> CurrentProduction =
    new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private City city;
    private HexGrid grid;
    private PlayerManager playerManager;
    private TerritorySystem territorySystem;


    private NetworkPlayerManager networkPlayerManager;
    private TurnManager turnManager;
    private BuildingSystem buildingSystem;

    private UnitProductionSystem unitProductionSystem;

    private Coroutine initializationCoroutine;
    private bool isInitialized;

    private void Awake()
    {
        city = GetComponent<City>();
        FindReferences();
    }

    private void FindReferences()
    {
        if (grid == null)
            grid = FindFirstObjectByType<HexGrid>();

        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        if (territorySystem == null)
            territorySystem = FindFirstObjectByType<TerritorySystem>();


        if (networkPlayerManager == null)
            networkPlayerManager =
                FindFirstObjectByType<NetworkPlayerManager>();

        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (buildingSystem == null)
            buildingSystem = FindFirstObjectByType<BuildingSystem>();

        if (unitProductionSystem == null)
        {
            unitProductionSystem =
                FindFirstObjectByType<UnitProductionSystem>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log(
          $"[NetworkCity] Spawn: " +
          $"Name={name}, " +
          $"Server={IsServer}, Client={IsClient}, " +
          $"ClientId={NetworkManager.LocalClientId}, " +
          $"NetworkObjectId={NetworkObjectId}, " +
          $"Owner={OwnerPlayerId.Value}, " +
          $"CityType={CityTypeId.Value}",
          this);

        OwnerPlayerId.OnValueChanged += OnNetworkDataChanged;
        CityTypeId.OnValueChanged += OnNetworkDataChanged;
        TileQ.OnValueChanged += OnNetworkDataChanged;
        TileR.OnValueChanged += OnNetworkDataChanged;

        CurrentProduction.OnValueChanged +=
        OnCurrentProductionChanged;

        if (IsServer && city != null)
        {
            city.OnProductionChanged +=
                OnCityProductionChanged;

            CurrentProduction.Value = city.CurrentProduction;
        }

        FindReferences();

        if (!IsServer)
            StartInitialization();
    }

    public override void OnNetworkDespawn()
    {
        OwnerPlayerId.OnValueChanged -= OnNetworkDataChanged;
        CityTypeId.OnValueChanged -= OnNetworkDataChanged;
        TileQ.OnValueChanged -= OnNetworkDataChanged;
        TileR.OnValueChanged -= OnNetworkDataChanged;

        if (initializationCoroutine != null)
        {
            StopCoroutine(initializationCoroutine);
            initializationCoroutine = null;
        }

        CurrentProduction.OnValueChanged -=
        OnCurrentProductionChanged;

        if (city != null)
        {
            city.OnProductionChanged -=
                OnCityProductionChanged;
        }

        base.OnNetworkDespawn();
    }

    // Викликається сервером до NetworkObject.Spawn().
    public void InitializeNetwork(
        Player owner,
        CityData data,
        HexTile tile,
        int cityTypeId)
    {
        if (!IsServer)
            return;

        if (owner == null || data == null || tile == null)
            return;

        OwnerPlayerId.Value = owner.PlayerId;
        CityTypeId.Value = cityTypeId;
        TileQ.Value = tile.Coordinates.Q;
        TileR.Value = tile.Coordinates.R;

        Debug.Log(
        $"[NetworkCity] InitializeNetwork: " +
        $"Owner={OwnerPlayerId.Value}, " +
        $"CityType={CityTypeId.Value}, " +
        $"Q={TileQ.Value}, R={TileR.Value}",
        this);

    }

    private void OnNetworkDataChanged(int previous, int current)
    {
        if (!IsServer && !isInitialized)
            StartInitialization();
    }

    private void StartInitialization()
    {
        if (IsServer || isInitialized || initializationCoroutine != null)
            return;

        initializationCoroutine = StartCoroutine(InitializeRoutine());
    }

    private IEnumerator InitializeRoutine()
    {
        while (IsSpawned && !IsServer && !isInitialized)
        {
            if (TryInitializeClientCity())
            {
                isInitialized = true;
                initializationCoroutine = null;
                yield break;
            }

            yield return null;
        }

        initializationCoroutine = null;
    }

    private void OnCityProductionChanged(int amount)
    {
        if (!IsServer || !IsSpawned)
            return;

        CurrentProduction.Value = amount;
    }

    private void OnCurrentProductionChanged(
        int previous,
        int current)
    {
        if (city == null)
            return;

        // Клієнт застосовує отримане від сервера значення.
        if (!IsServer && city.Data != null)
        {
            city.SetProductionFromNetwork(current);
        }
    }

    private bool TryInitializeClientCity()
    {
        if (city == null)
            return false;

        FindReferences();

        if (grid == null || playerManager == null)
            return false;

        if (OwnerPlayerId.Value < 0 || CityTypeId.Value < 0)
            return false;

        Player owner = playerManager.GetPlayer(OwnerPlayerId.Value);

        if (owner == null)
            return false;

        CivilizationData civilization = owner.GetCivilization();

        if (civilization == null ||
            civilization.cityDatabase == null)
        {
            return false;
        }

        var cities = civilization.cityDatabase.Cities;

        if (CityTypeId.Value >= cities.Count)
        {
            Debug.LogError(
                $"[NetworkCity] Invalid CityTypeId: {CityTypeId.Value}.",
                this);
            return false;
        }

        CityData cityData = cities[CityTypeId.Value];

        if (cityData == null)
            return false;

        HexCoordinates coordinates =
            new HexCoordinates(TileQ.Value, TileR.Value);

        HexTile tile = grid.GetTile(coordinates);

        if (tile == null)
        {
            Debug.LogWarning(
                $"[NetworkCity] Client cannot find tile " +
                $"Q={TileQ.Value}, R={TileR.Value}. " +
                $"City={name}",
                this);

            return false;
        }

        if (city.Data == null)
        {
            city.Initialize(cityData, owner, tile);
        }

        if (city.Data != cityData ||
            city.Owner != owner ||
            city.CenterTile != tile)
        {
            return false;
        }

        if (territorySystem == null)
        {
            Debug.LogError(
                "[NetworkCity] TerritorySystem is missing.",
                this);

            return false;
        }

        if (city.Territory.Count == 0)
        {
            territorySystem.GenerateTerritory(city);
        }

        // Відновлюємо зв'язок, якщо він не був встановлений.
        if (!tile.HasCity())
        {
            tile.SetCity(city);
        }

        if (tile.City != city)
        {
            Debug.LogError(
                "[NetworkCity] City is registered incorrectly on the tile.",
                this);

            return false;
        }

        city.SetProductionFromNetwork(
        CurrentProduction.Value);

        return true;
    }

    public bool RequestBuild(int tileQ, int tileR, int buildingTypeId)
    {
        if (!IsSpawned || !IsClient)
            return false;

        if (IsServer)
        {
            return ProcessBuildRequest(
                city != null ? city.Owner : null,
                tileQ,
                tileR,
                buildingTypeId);
        }

        RequestBuildRpc(tileQ, tileR, buildingTypeId);
        return true;
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestBuildRpc(
    int tileQ,
    int tileR,
    int buildingTypeId,
    RpcParams rpcParams = default)
    {
        if (!IsServer || !IsSpawned)
            return;

        FindReferences();

        ulong senderClientId =
            rpcParams.Receive.SenderClientId;

        Debug.Log(
            $"[NetworkCity] Build request received. " +
            $"SenderClientId={senderClientId}, " +
            $"Tile=({tileQ}, {tileR}), " +
            $"BuildingTypeId={buildingTypeId}",
            this);

        if (networkPlayerManager == null)
        {
            Debug.LogError(
                "[NetworkCity] NetworkPlayerManager is missing.",
                this);
            return;
        }

        Player requestingPlayer =
            networkPlayerManager.GetPlayer(senderClientId);

        if (requestingPlayer == null)
        {
            Debug.LogError(
                $"[NetworkCity] Player not found for " +
                $"ClientId={senderClientId}.",
                this);
            return;
        }

        bool success = ProcessBuildRequest(
            requestingPlayer,
            tileQ,
            tileR,
            buildingTypeId);

        Debug.Log(
            $"[NetworkCity] Build request result: {success}. " +
            $"PlayerId={requestingPlayer.PlayerId}",
            this);
    }

    private bool ProcessBuildRequest(
    Player requestingPlayer,
    int tileQ,
    int tileR,
    int buildingTypeId)
    {
        if (!IsServer || city == null)
        {
            Debug.LogError(
                "[NetworkCity] Server or City is missing.",
                this);
            return false;
        }

        FindReferences();

        if (requestingPlayer == null)
        {
            Debug.LogError(
                "[NetworkCity] Requesting player is null.",
                this);
            return false;
        }

        if (requestingPlayer != city.Owner)
        {
            Debug.LogWarning(
                $"[NetworkCity] Rejected: player {requestingPlayer.PlayerId} " +
                $"does not own this city. Owner=" +
                $"{(city.Owner != null ? city.Owner.PlayerId : -1)}",
                this);
            return false;
        }

        if (turnManager == null)
        {
            Debug.LogError(
                "[NetworkCity] TurnManager is missing.",
                this);
            return false;
        }

        if (turnManager.CurrentPlayer != requestingPlayer)
        {
            Debug.LogWarning(
                $"[NetworkCity] Rejected: wrong turn. " +
                $"CurrentPlayerId=" +
                $"{(turnManager.CurrentPlayer != null ? turnManager.CurrentPlayer.PlayerId : -1)}, " +
                $"RequestingPlayerId={requestingPlayer.PlayerId}",
                this);
            return false;
        }

        if (buildingSystem == null || grid == null)
        {
            Debug.LogError(
                "[NetworkCity] BuildingSystem or HexGrid is missing.",
                this);
            return false;
        }

        CivilizationData civilization =
            requestingPlayer.GetCivilization();

        if (civilization == null ||
            civilization.buildingDatabase == null)
        {
            Debug.LogError(
                "[NetworkCity] Civilization or BuildingDatabase is missing.",
                this);
            return false;
        }

        var buildings =
            civilization.buildingDatabase.Buildings;

        if (buildingTypeId < 0 ||
            buildingTypeId >= buildings.Count)
        {
            Debug.LogError(
                $"[NetworkCity] Invalid BuildingTypeId={buildingTypeId}. " +
                $"Available buildings={buildings.Count}",
                this);
            return false;
        }

        BuildingData data = buildings[buildingTypeId];

        if (data == null)
        {
            Debug.LogError(
                "[NetworkCity] BuildingData is null.",
                this);
            return false;
        }

        HexTile tile = grid.GetTile(
            new HexCoordinates(tileQ, tileR));

        if (tile == null)
        {
            Debug.LogError(
                $"[NetworkCity] Tile not found: Q={tileQ}, R={tileR}.",
                this);
            return false;
        }

        bool success = buildingSystem.Build(
            requestingPlayer,
            city,
            tile,
            data);

        if (!success)
        {
            Debug.LogWarning(
                $"[NetworkCity] BuildingSystem.Build rejected " +
                $"'{data.buildingName}' at ({tileQ}, {tileR}). " +
                "Check terrain, territory, technology, resources, " +
                "city build limit and prefab configuration.",
                this);
        }

        return success;
    }

    public bool RequestProduceUnit(int unitTypeId)
    {
        if (!IsSpawned || !IsClient)
            return false;

        if (unitTypeId < 0)
            return false;

        // Host виконує запит без RPC, але з тією самою серверною перевіркою.
        if (IsServer)
        {
            return ProcessProduceRequest(
                city != null ? city.Owner : null,
                unitTypeId);
        }

        RequestProduceUnitRpc(unitTypeId);
        return true;
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void RequestProduceUnitRpc(
        int unitTypeId,
        RpcParams rpcParams = default)
    {
        if (!IsServer || !IsSpawned)
            return;

        FindReferences();

        if (networkPlayerManager == null)
        {
            Debug.LogError(
                "[NetworkCity] NetworkPlayerManager is missing.");
            return;
        }

        Player requestingPlayer =
            networkPlayerManager.GetPlayer(
                rpcParams.Receive.SenderClientId);

        if (requestingPlayer == null)
        {
            Debug.LogWarning(
                "[NetworkCity] Production request rejected: player not found.");
            return;
        }

        ProcessProduceRequest(requestingPlayer, unitTypeId);
    }

    private bool ProcessProduceRequest(
        Player requestingPlayer,
        int unitTypeId)
    {
        if (!IsServer || !IsSpawned || city == null)
            return false;

        FindReferences();

        if (requestingPlayer == null ||
            requestingPlayer != city.Owner)
        {
            Debug.LogWarning(
                "[NetworkCity] Production request rejected: city ownership mismatch.");
            return false;
        }

        if (turnManager == null ||
            turnManager.CurrentPlayer != requestingPlayer)
        {
            Debug.LogWarning(
                "[NetworkCity] Production request rejected: wrong turn.");
            return false;
        }

        CivilizationData civilization =
            requestingPlayer.GetCivilization();

        if (civilization == null ||
            civilization.unitDatabase == null)
        {
            Debug.LogError(
                "[NetworkCity] UnitDatabase is missing.");
            return false;
        }

        var units = civilization.unitDatabase.Units;

        if (unitTypeId < 0 || unitTypeId >= units.Count)
        {
            Debug.LogWarning(
                $"[NetworkCity] Invalid UnitTypeId={unitTypeId}.");
            return false;
        }

        UnitData unitData = units[unitTypeId];

        if (unitData == null)
            return false;

        if (unitProductionSystem == null)
        {
            Debug.LogError(
                "[NetworkCity] UnitProductionSystem is missing.");
            return false;
        }

        return unitProductionSystem.ProduceOnServer(
            requestingPlayer,
            city,
            unitData);
    }
}
