using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkUnit : NetworkBehaviour
{
    // =========================================================
    // NETWORK STATE
    // =========================================================

    public NetworkVariable<int> PlayerId =
        new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> UnitTypeId =
        new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> CurrentHealth =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> MovementPoints =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> TileQ =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> TileR =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );


    // =========================================================
    // REFERENCES
    // =========================================================

    private Unit unit;

    private HexGrid grid;
    private PlayerManager playerManager;
    private NetworkPlayerManager networkPlayerManager;
    private TurnManager turnManager;
    private FeatureSystem featureSystem;
    private Coroutine initializationCoroutine;

    private Coroutine tileUpdateCoroutine;

    public event System.Action<Unit> OnNetworkTileUpdated;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        unit = GetComponent<Unit>();

        FindReferences();
    }


    // =========================================================
    // REFERENCES
    // =========================================================

    private void FindReferences()
    {
        if (grid == null)
        {
            grid = FindFirstObjectByType<HexGrid>();
        }

        if (playerManager == null)
        {
            playerManager =
                FindFirstObjectByType<PlayerManager>();
        }

        if (networkPlayerManager == null)
        {
            networkPlayerManager =
                FindFirstObjectByType<NetworkPlayerManager>();
        }

        if (turnManager == null)
        {
            turnManager =
                FindFirstObjectByType<TurnManager>();
        }

        if (featureSystem == null)
        {
            featureSystem = FindFirstObjectByType<FeatureSystem>();
        }
    }


    // =========================================================
    // NETWORK SPAWN
    // =========================================================

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SubscribeToNetworkVariables();

        FindReferences();

        if (IsServer)
        {
            InitializeServerState();
        }
        else
        {
            StartInitialization();
        }
    }


    public override void OnNetworkDespawn()
    {
        UnsubscribeFromNetworkVariables();

        StopInitializationCoroutine();

        if (tileUpdateCoroutine != null)
        {
            StopCoroutine(tileUpdateCoroutine);
            tileUpdateCoroutine = null;
        }

        base.OnNetworkDespawn();
    }


    // =========================================================
    // SUBSCRIPTIONS
    // =========================================================

    private void SubscribeToNetworkVariables()
    {
        PlayerId.OnValueChanged +=
            OnPlayerIdChanged;

        UnitTypeId.OnValueChanged +=
            OnUnitTypeIdChanged;

        CurrentHealth.OnValueChanged +=
            OnHealthChanged;

        MovementPoints.OnValueChanged +=
            OnMovementPointsChanged;

        TileQ.OnValueChanged +=
            OnTileChanged;

        TileR.OnValueChanged +=
            OnTileChanged;
    }


    private void UnsubscribeFromNetworkVariables()
    {
        PlayerId.OnValueChanged -=
            OnPlayerIdChanged;

        UnitTypeId.OnValueChanged -=
            OnUnitTypeIdChanged;

        CurrentHealth.OnValueChanged -=
            OnHealthChanged;

        MovementPoints.OnValueChanged -=
            OnMovementPointsChanged;

        TileQ.OnValueChanged -=
            OnTileChanged;

        TileR.OnValueChanged -=
            OnTileChanged;
    }


    // =========================================================
    // SERVER INITIALIZATION
    // =========================================================

    private void InitializeServerState()
    {
        if (!IsServer)
        {
            return;
        }

        SyncStateFromUnit();
    }


    /// <summary>
    /// Called by UnitManager after the unit has been created.
    /// </summary>
    public void InitializeNetwork()
    {
        if (!IsServer)
        {
            return;
        }

        SyncStateFromUnit();
    }


    /// <summary>
    /// Copies the authoritative Unit state into NetworkVariables.
    /// Only the server is allowed to do this.
    /// </summary>
    private void SyncStateFromUnit()
    {
        if (!IsServer)
        {
            return;
        }

        if (unit == null)
        {
            return;
        }

        // -----------------------------------------------------
        // OWNER
        // -----------------------------------------------------

        if (unit.Owner != null)
        {
            PlayerId.Value =
                unit.Owner.PlayerId;
        }

        // -----------------------------------------------------
        // UNIT TYPE
        // -----------------------------------------------------

        if (unit.Data != null)
        {
            UnitTypeId.Value =
                unit.Data.UnitId;
        }

        // -----------------------------------------------------
        // HEALTH
        // -----------------------------------------------------

        CurrentHealth.Value =
            unit.CurrentHealth;

        // -----------------------------------------------------
        // MOVEMENT
        // -----------------------------------------------------

        MovementPoints.Value =
            unit.MovementPoints;

        // -----------------------------------------------------
        // TILE
        // -----------------------------------------------------

        if (unit.CurrentTile != null)
        {
            TileQ.Value =
                unit.CurrentTile.Coordinates.Q;

            TileR.Value =
                unit.CurrentTile.Coordinates.R;
        }
    }


    // =========================================================
    // CLIENT INITIALIZATION
    // =========================================================

    private void StartInitialization()
    {
        if (IsServer)
        {
            return;
        }

        StopInitializationCoroutine();

        initializationCoroutine =
            StartCoroutine(
                InitializeRoutine()
            );
    }


    private void StopInitializationCoroutine()
    {
        if (initializationCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            initializationCoroutine
        );

        initializationCoroutine = null;
    }


    private IEnumerator InitializeRoutine()
    {
        while (IsSpawned && !IsServer)
        {
            if (TryInitializeClientUnit())
            {
                initializationCoroutine = null;

                yield break;
            }

            yield return null;
        }

        initializationCoroutine = null;
    }


    /// <summary>
    /// Creates the local Unit representation using
    /// synchronized PlayerId and UnitTypeId.
    /// </summary>
    private bool TryInitializeClientUnit()
    {
        if (IsServer)
        {
            return false;
        }

        if (unit == null)
        {
            return false;
        }

        FindReferences();

        if (grid == null)
        {
            return false;
        }

        if (playerManager == null)
        {
            return false;
        }

        if (PlayerId.Value < 0)
        {
            return false;
        }

        if (UnitTypeId.Value < 0)
        {
            return false;
        }

        // -----------------------------------------------------
        // FIND PLAYER
        // -----------------------------------------------------

        Player player =
            playerManager.GetPlayer(
                PlayerId.Value
            );

        if (player == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // FIND CIVILIZATION
        // -----------------------------------------------------

        CivilizationData civilization =
            player.GetCivilization();

        if (civilization == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // FIND UNIT DATABASE
        // -----------------------------------------------------

        if (civilization.unitDatabase == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // FIND UNIT DATA
        // -----------------------------------------------------

        UnitData unitData =
            civilization.unitDatabase.GetUnit(
                UnitTypeId.Value
            );

        if (unitData == null)
        {
            Debug.LogError(
                $"[NetworkUnit] UnitData " +
                $"{UnitTypeId.Value} was not found " +
                $"in civilization " +
                $"{civilization.civilizationName}.",
                this
            );

            return false;
        }

        // -----------------------------------------------------
        // FIND TILE
        // -----------------------------------------------------

        HexCoordinates coordinates =
            new HexCoordinates(
                TileQ.Value,
                TileR.Value
            );

        HexTile tile =
            grid.GetTile(coordinates);

        if (tile == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // INITIALIZE UNIT
        // -----------------------------------------------------

        bool needsInitialization =
            unit.Owner != player ||
            unit.Data != unitData ||
            unit.CurrentTile != tile;

        if (needsInitialization)
        {
            unit.Initialize(
                player,
                unitData,
                tile,
                grid
            );
        }

        // -----------------------------------------------------
        // APPLY NETWORK STATE
        // -----------------------------------------------------

        ApplyNetworkState();

        return true;
    }


    // =========================================================
    // APPLY NETWORK STATE
    // =========================================================

    private void ApplyNetworkState()
    {
        if (unit == null)
        {
            return;
        }

        unit.SetNetworkHealth(
            CurrentHealth.Value
        );

        unit.SetNetworkMovementPoints(
            MovementPoints.Value
        );

        ApplyNetworkTile();
    }


    private void ApplyNetworkTile()
    {
        if (unit == null)
        {
            return;
        }

        FindReferences();

        if (grid == null)
        {
            return;
        }

        HexCoordinates coordinates =
            new HexCoordinates(
                TileQ.Value,
                TileR.Value
            );

        HexTile tile =
            grid.GetTile(coordinates);

        if (tile == null)
        {
            return;
        }

        if (unit.CurrentTile != tile)
        {
            unit.NetworkSetCurrentTile(tile);
        }

        OnNetworkTileUpdated?.Invoke(unit);
    }


    // =========================================================
    // NETWORK VARIABLE CALLBACKS
    // =========================================================

    private void OnPlayerIdChanged(
        int previous,
        int current)
    {
        if (IsServer)
        {
            return;
        }

        StartInitialization();
    }


    private void OnUnitTypeIdChanged(
        int previous,
        int current)
    {
        if (IsServer)
        {
            return;
        }

        StartInitialization();
    }


    private void OnHealthChanged(
        int previous,
        int current)
    {
        if (IsServer)
        {
            return;
        }

        if (unit == null)
        {
            return;
        }

        unit.SetNetworkHealth(current);
    }


    private void OnMovementPointsChanged(int previous, int current)
    {
        Debug.Log(
            $"[NetworkUnit] MovementPoints changed: " +
            $"{previous} -> {current}, IsServer: {IsServer}, " +
            $"Unit: {unit?.name}"
        );

        if (IsServer || unit == null)
            return;

        unit.SetNetworkMovementPoints(current);
    }


    private void OnTileChanged(int previous, int current)
    {
        if (IsServer)
            return;

        ScheduleTileUpdate();
    }

    private void ScheduleTileUpdate()
    {
        if (!IsSpawned || IsServer)
            return;

        // Якщо координати оновилися кілька разів за кадр,
        // виконуємо перерахунок лише один раз.
        if (tileUpdateCoroutine != null)
            return;

        tileUpdateCoroutine = StartCoroutine(
            ApplyTileUpdateNextFrame()
        );
    }

    private IEnumerator ApplyTileUpdateNextFrame()
    {
        // Дозволяємо обом координатам оновитися.
        yield return null;

        tileUpdateCoroutine = null;

        if (!IsSpawned || IsServer)
            yield break;

        ApplyNetworkTile();
    }


    // =========================================================
    // MOVE REQUEST
    // =========================================================

    /// <summary>
    /// Called by the local Unit/Selection system.
    /// The client only requests movement.
    /// The server decides whether the movement is legal.
    /// </summary>
    public void RequestMove(
        int targetQ,
        int targetR)
    {
        if (!IsClient)
        {
            return;
        }

        if (!IsSpawned)
        {
            return;
        }

        RequestMoveRpc(
            targetQ,
            targetR
        );
    }


    [Rpc(SendTo.Server)]
    private void RequestMoveRpc(
        int targetQ,
        int targetR,
        RpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        ulong clientId =
            rpcParams.Receive.SenderClientId;

        if (!ValidateMoveRequest(
                clientId,
                targetQ,
                targetR))
        {
            return;
        }

        HexCoordinates targetCoordinates =
            new HexCoordinates(
                targetQ,
                targetR
            );

        HexTile targetTile =
            grid.GetTile(targetCoordinates);

        if (targetTile == null)
        {
            return;
        }

        UnitMovement movement =
            unit.GetComponent<UnitMovement>();

        if (movement == null)
        {
            Debug.LogError(
                "[NetworkUnit] UnitMovement " +
                "component is missing.",
                this
            );

            return;
        }

        // -----------------------------------------------------
        // AUTHORITATIVE SERVER MOVE
        // -----------------------------------------------------

        if (!movement.TryMoveTo(targetTile))
        {
            return;
        }

        // -----------------------------------------------------
        // UPDATE NETWORK STATE
        // -----------------------------------------------------

        SyncStateFromUnit();

        ApplyNetworkTile();
    }


    // =========================================================
    // MOVE VALIDATION
    // =========================================================

    private bool ValidateMoveRequest(
        ulong clientId,
        int targetQ,
        int targetR)
    {
        if (!IsServer)
        {
            return false;
        }

        if (unit == null)
        {
            return false;
        }

        if (unit.Owner == null)
        {
            return false;
        }

        FindReferences();

        if (grid == null)
        {
            return false;
        }

        if (networkPlayerManager == null)
        {
            return false;
        }

        if (turnManager == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // PLAYER
        // -----------------------------------------------------

        Player player =
            networkPlayerManager.GetPlayer(
                clientId
            );

        if (player == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // OWNERSHIP
        // -----------------------------------------------------

        if (player != unit.Owner)
        {
            return false;
        }

        // -----------------------------------------------------
        // TURN
        // -----------------------------------------------------

        if (turnManager.CurrentPlayer == null)
        {
            return false;
        }

        if (turnManager.CurrentPlayer != unit.Owner)
        {
            return false;
        }

        // -----------------------------------------------------
        // TARGET TILE
        // -----------------------------------------------------

        HexCoordinates targetCoordinates =
            new HexCoordinates(
                targetQ,
                targetR
            );

        HexTile targetTile =
            grid.GetTile(targetCoordinates);

        if (targetTile == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // OCCUPIED TILE
        // -----------------------------------------------------

        if (targetTile.IsOccupied())
        {
            return false;
        }

        // -----------------------------------------------------
        // MOVEMENT SYSTEM
        // -----------------------------------------------------

        UnitMovement movement =
            unit.GetComponent<UnitMovement>();

        if (movement == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // MOVEMENT RULES
        // -----------------------------------------------------

        if (!movement.CanMoveTo(targetTile))
        {
            return false;
        }

        return true;
    }

    public bool RequestActivateFeature(
    Feature feature,
    Unit target = null)
    {
        if (feature == null || unit == null)
            return false;

        if (!IsSpawned || !IsClient)
            return false;

        if (unit.Data == null || unit.Data.Features == null)
            return false;

        // Знаходимо індекс здібності в UnitData.
        int featureIndex = -1;

        for (int i = 0; i < unit.Data.Features.Count; i++)
        {
            if (unit.Data.Features[i] == feature)
            {
                featureIndex = i;
                break;
            }
        }

        if (featureIndex < 0)
            return false;

        bool hasTarget = target != null;
        ulong targetNetworkObjectId = 0;

        if (hasTarget)
        {
            NetworkObject targetNetworkObject =
                target.GetComponent<NetworkObject>();

            if (targetNetworkObject == null ||
                !targetNetworkObject.IsSpawned)
            {
                return false;
            }

            targetNetworkObjectId =
                targetNetworkObject.NetworkObjectId;
        }

        if (feature.RequiresTarget && !hasTarget)
            return false;

        RequestActivateFeatureRpc(
            featureIndex,
            hasTarget,
            targetNetworkObjectId
        );

        return true;
    }


    [Rpc(SendTo.Server)]
    private void RequestActivateFeatureRpc(
        int featureIndex,
        bool hasTarget,
        ulong targetNetworkObjectId,
        RpcParams rpcParams = default)
    {
        if (!IsServer || !IsSpawned || unit == null)
            return;

        FindReferences();

        if (featureSystem == null ||
            networkPlayerManager == null ||
            turnManager == null ||
            unit.Owner == null ||
            unit.Data == null)
        {
            return;
        }

        // Перевіряємо, хто надіслав запит.
        ulong clientId =
            rpcParams.Receive.SenderClientId;

        Player requestingPlayer =
            networkPlayerManager.GetPlayer(clientId);

        if (requestingPlayer == null ||
            requestingPlayer != unit.Owner)
        {
            return;
        }

        // Перевіряємо хід.
        if (turnManager.CurrentPlayer != unit.Owner)
            return;

        // Перевіряємо індекс здібності.
        if (featureIndex < 0 ||
            featureIndex >= unit.Data.Features.Count)
        {
            return;
        }

        Feature feature =
            unit.Data.Features[featureIndex];

        if (feature == null || !feature.IsActive)
            return;

        // Знаходимо ціль на сервері.
        Unit targetUnit = null;

        if (hasTarget)
        {
            if (!NetworkManager.SpawnManager.SpawnedObjects
                .TryGetValue(
                    targetNetworkObjectId,
                    out NetworkObject targetNetworkObject))
            {
                return;
            }

            targetUnit =
                targetNetworkObject.GetComponent<Unit>();

            if (targetUnit == null)
                return;
        }

        if (feature.RequiresTarget && targetUnit == null)
            return;

        if (!feature.RequiresTarget && targetUnit != null)
            return;

        FeatureTarget target = targetUnit != null
            ? new FeatureTarget(unit: targetUnit)
            : null;

        FeatureContext context = new FeatureContext(
            unit: unit,
            player: unit.Owner,
            target: target,
            effectSystem: featureSystem.EffectSystem
        );

        // Уся ігрова логіка виконується на сервері.
        if (!featureSystem.CanActivate(feature, context))
            return;

        if (!featureSystem.Activate(feature, context))
            return;

        // Синхронізуємо здоров'я та інші мережеві стани.
        SyncStateFromUnit();

        if (targetUnit != null)
        {
            NetworkUnit targetNetworkUnit =
                targetUnit.GetComponent<NetworkUnit>();

            if (targetNetworkUnit != null &&
                targetNetworkUnit.IsSpawned)
            {
                targetNetworkUnit.ServerSyncState();
            }
        }
    }

    public void ServerSyncState()
    {
        if (!IsServer || !IsSpawned)
            return;

        SyncStateFromUnit();
    }
}