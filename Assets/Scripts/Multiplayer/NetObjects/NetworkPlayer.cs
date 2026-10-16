
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<int> PlayerId =
        new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SyncedFood =
    new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SyncedProduction =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SyncedGold =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> SyncedScience =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private PlayerResources subscribedResources;

    // Кожен NetworkPlayer зберігає дослідження
    // тільки свого гравця.
    private NetworkList<int> researchedTechnologyIds;

    public Player Player { get; private set; }

    public bool IsAssigned => PlayerId.Value >= 0;

    private PlayerManager playerManager;
    private TurnManager turnManager;
    private TechnologySystem technologySystem;

    private void Awake()
    {
        researchedTechnologyIds = new NetworkList<int>();
        FindReferences();
    }

    private void FindReferences()
    {
        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (technologySystem == null)
            technologySystem = FindFirstObjectByType<TechnologySystem>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        PlayerId.OnValueChanged += OnPlayerIdChanged;
        researchedTechnologyIds.OnListChanged +=
            OnResearchedTechnologiesChanged;

        FindReferences();
        TryLinkPlayer();

        SyncedFood.OnValueChanged += OnNetworkResourcesChanged;
        SyncedProduction.OnValueChanged += OnNetworkResourcesChanged;
        SyncedGold.OnValueChanged += OnNetworkResourcesChanged;
        SyncedScience.OnValueChanged += OnNetworkResourcesChanged;

        RefreshResourceSubscription();

        // Відновлюємо стан, навіть якщо список
        // уже був заповнений до появи цього клієнта.
        ApplyAllResearchedTechnologies();

        Debug.Log(
            $"[NetworkPlayer] Spawned. " +
            $"ClientId={OwnerClientId}, " +
            $"LocalClientId={NetworkManager.LocalClientId}, " +
            $"IsOwner={IsOwner}, " +
            $"PlayerId={PlayerId.Value}",
            this);
    }

    public override void OnNetworkDespawn()
    {
        PlayerId.OnValueChanged -= OnPlayerIdChanged;

        researchedTechnologyIds.OnListChanged -=
            OnResearchedTechnologiesChanged;

        SyncedFood.OnValueChanged -= OnNetworkResourcesChanged;
        SyncedProduction.OnValueChanged -= OnNetworkResourcesChanged;
        SyncedGold.OnValueChanged -= OnNetworkResourcesChanged;
        SyncedScience.OnValueChanged -= OnNetworkResourcesChanged;

        if (subscribedResources != null)
        {
            subscribedResources.OnResourcesChanged -= OnLocalResourcesChanged;
            subscribedResources = null;
        }

        base.OnNetworkDespawn();
    }

    private void RefreshResourceSubscription()
    {
        PlayerResources currentResources = Player?.Resources;

        if (subscribedResources != currentResources)
        {
            if (subscribedResources != null)
            {
                subscribedResources.OnResourcesChanged -=
                    OnLocalResourcesChanged;
            }

            subscribedResources = currentResources;

            if (IsServer && subscribedResources != null)
            {
                subscribedResources.OnResourcesChanged +=
                    OnLocalResourcesChanged;
            }
        }

        if (IsServer)
        {
            SyncResourcesToNetwork();
        }
        else
        {
            ApplyNetworkResourcesToPlayer();
        }
    }

    private void OnLocalResourcesChanged()
    {
        if (!IsServer)
            return;

        SyncResourcesToNetwork();
    }

    private void SyncResourcesToNetwork()
    {
        if (!IsServer || Player?.Resources == null)
            return;

        PlayerResources resources = Player.Resources;

        if (SyncedFood.Value != resources.Food)
            SyncedFood.Value = resources.Food;

        if (SyncedProduction.Value != resources.Production)
            SyncedProduction.Value = resources.Production;

        if (SyncedGold.Value != resources.Gold)
            SyncedGold.Value = resources.Gold;

        if (SyncedScience.Value != resources.Science)
            SyncedScience.Value = resources.Science;
    }

    private void OnNetworkResourcesChanged(int previous, int current)
    {
        if (IsServer)
            return;

        ApplyNetworkResourcesToPlayer();
    }

    private void ApplyNetworkResourcesToPlayer()
    {
        if (IsServer || Player?.Resources == null)
            return;

        Player.Resources.SetValues(
            SyncedFood.Value,
            SyncedProduction.Value,
            SyncedGold.Value,
            SyncedScience.Value);
    }

    public void AssignPlayerId(int playerId)
    {
        if (!IsServer)
            return;

        if (playerId < 0)
        {
            Debug.LogError(
                "[NetworkPlayer] PlayerId cannot be negative.");
            return;
        }

        PlayerId.Value = playerId;
        TryLinkPlayer();
    }

    public void AssignPlayer(Player player)
    {
        if (player == null)
        {
            Debug.LogError(
                "[NetworkPlayer] Cannot assign null Player.");
            return;
        }

        Player = player;
        UpdateLocalPlayerReference();
        RefreshResourceSubscription();

        if (IsServer)
        {
            // Новий мережевий об'єкт отримує прогрес
            // уже наявного ігрового гравця.
            SyncResearchedTechnologiesFromPlayer();
        }
        else
        {
            // Клієнт відновлює стан із мережевого списку.
            RefreshLocalTechnologies();
        }

        Debug.Log(
            $"[NetworkPlayer] ClientId={OwnerClientId} " +
            $"linked to PlayerId={player.PlayerId}.",
            this);
    }

    private void RefreshLocalTechnologies()
    {
        if (Player == null || researchedTechnologyIds == null)
            return;

        // На клієнті прибираємо технології попереднього
        // стану локального Player і відновлюємо актуальні.
        if (!IsServer)
            Player.ClearTechnologies();

        foreach (int technologyId in researchedTechnologyIds)
        {
            ApplyTechnologyId(technologyId, false);
        }

        Debug.Log(
            $"[NetworkPlayer] Refreshed technologies for " +
            $"PlayerId={Player.PlayerId}. " +
            $"Count={researchedTechnologyIds.Count}",
            this);
    }

    private void OnPlayerIdChanged(int previous, int current)
    {
        TryLinkPlayer();
        ApplyAllResearchedTechnologies();
    }

    private void TryLinkPlayer()
    {
        FindReferences();

        if (PlayerId.Value < 0 || playerManager == null)
            return;

        Player player = playerManager.GetPlayer(PlayerId.Value);

        if (player == null)
        {
            Debug.LogWarning(
                $"[NetworkPlayer] Player {PlayerId.Value} " +
                "is not available yet.",
                this);
            return;
        }

        if (Player != player)
            AssignPlayer(player);
        else
        {
            RefreshResourceSubscription();

            if (IsServer)
                SyncResearchedTechnologiesFromPlayer();
            else
                RefreshLocalTechnologies();
        }

        UpdateLocalPlayerReference();
    }

    /// <summary>
    /// Викликається TechnologySystem після натискання
    /// кнопки дослідження.
    /// </summary>
    public bool RequestResearchTechnology(int technologyId)
    {
        if (!IsSpawned || !IsClient || !IsOwner)
            return false;

        if (technologyId < 0)
            return false;

        // Для Host обробляємо запит локально на сервері.
        if (IsServer)
        {
            return ProcessResearchRequest(
                OwnerClientId,
                technologyId);
        }

        RequestResearchTechnologyRpc(technologyId);
        return true;
    }

    [Rpc(SendTo.Server)]
    private void RequestResearchTechnologyRpc(
        int technologyId,
        RpcParams rpcParams = default)
    {
        if (!IsServer || !IsSpawned)
            return;

        ulong senderClientId =
            rpcParams.Receive.SenderClientId;

        ProcessResearchRequest(
            senderClientId,
            technologyId);
    }

    public void ResetResearchState()
    {
        if (!IsServer)
            return;

        researchedTechnologyIds.Clear();

        if (Player != null)
            Player.ClearTechnologies();
    }

    private bool ProcessResearchRequest(
        ulong senderClientId,
        int technologyId)
    {
        if (!IsServer || !IsSpawned)
            return false;

        // Захист від запитів від імені іншого гравця.
        if (senderClientId != OwnerClientId)
        {
            Debug.LogWarning(
                "[NetworkPlayer] Rejected research request: " +
                "sender does not own this NetworkPlayer.",
                this);
            return false;
        }

        FindReferences();

        if (playerManager == null ||
            turnManager == null ||
            technologySystem == null)
        {
            Debug.LogError(
                "[NetworkPlayer] Required references are missing.",
                this);
            return false;
        }

        Player player = playerManager.GetPlayer(PlayerId.Value);

        if (player == null)
        {
            Debug.LogError(
                $"[NetworkPlayer] Player {PlayerId.Value} not found.",
                this);
            return false;
        }

        if (turnManager.CurrentPlayer != player)
        {
            Debug.LogWarning(
                $"[NetworkPlayer] Research rejected: it is not " +
                $"Player {player.PlayerId}'s turn.",
                this);
            return false;
        }

        CivilizationData civilization = player.GetCivilization();

        if (civilization == null ||
            civilization.technologyDatabase == null)
        {
            Debug.LogError(
                "[NetworkPlayer] TechnologyDatabase is missing.",
                this);
            return false;
        }

        var technologies =
            civilization.technologyDatabase.Technologies;

        if (technologyId < 0 ||
            technologyId >= technologies.Count)
        {
            Debug.LogWarning(
                $"[NetworkPlayer] Invalid TechnologyId={technologyId}.",
                this);
            return false;
        }

        TechnologyData technology = technologies[technologyId];

        if (technology == null)
            return false;

        // Усі перевірки та списання науки виконуються на сервері.
        if (!technologySystem.ResearchAuthoritative(
                player,
                technology))
        {
            Debug.LogWarning(
                $"[NetworkPlayer] Research rejected for " +
                $"'{technology.technologyName}'.",
                this);
            return false;
        }

        if (!researchedTechnologyIds.Contains(technologyId))
            researchedTechnologyIds.Add(technologyId);

        Debug.Log(
            $"[NetworkPlayer] Player {player.PlayerId} researched " +
            $"'{technology.technologyName}'.",
            this);

        return true;
    }

    private void OnResearchedTechnologiesChanged(
    NetworkListEvent<int> changeEvent)
    {
        if (IsServer || Player == null)
            return;

        switch (changeEvent.Type)
        {
            case NetworkListEvent<int>.EventType.Add:
            case NetworkListEvent<int>.EventType.Insert:
            case NetworkListEvent<int>.EventType.Value:
                ApplyTechnologyId(changeEvent.Value, true);
                break;

            case NetworkListEvent<int>.EventType.Remove:
            case NetworkListEvent<int>.EventType.RemoveAt:
            case NetworkListEvent<int>.EventType.Clear:
                Player.ClearTechnologies();
                ApplyAllResearchedTechnologies();
                break;
        }
    }

    private void ApplyAllResearchedTechnologies()
    {
        if (researchedTechnologyIds == null)
            return;

        foreach (int technologyId in researchedTechnologyIds)
            ApplyTechnologyId(technologyId, false);
    }

    private void ApplyTechnologyId(
    int technologyId,
    bool notifyUI)
    {
        FindReferences();

        if (Player == null)
            return;

        CivilizationData civilization = Player.GetCivilization();

        if (civilization == null ||
            civilization.technologyDatabase == null)
            return;

        var technologies =
            civilization.technologyDatabase.Technologies;

        if (technologyId < 0 ||
            technologyId >= technologies.Count)
            return;

        TechnologyData technology = technologies[technologyId];

        if (technology == null)
            return;

        bool wasResearched = Player.HasTechnology(technology);

        Player.AddTechnology(technology);

        if (notifyUI && !wasResearched && technologySystem != null)
        {
            technologySystem.NotifyTechnologySynchronized(
                Player,
                technology);
        }
    }

    private void SyncResearchedTechnologiesFromPlayer()
    {
        if (!IsServer || Player == null || researchedTechnologyIds == null)
            return;

        researchedTechnologyIds.Clear();

        CivilizationData civilization = Player.GetCivilization();

        if (civilization == null ||
            civilization.technologyDatabase == null)
            return;

        var technologies = civilization.technologyDatabase.Technologies;

        for (int i = 0; i < technologies.Count; i++)
        {
            TechnologyData technology = technologies[i];

            if (technology != null && Player.HasTechnology(technology))
            {
                researchedTechnologyIds.Add(i);
            }
        }

        Debug.Log(
            $"[NetworkPlayer] Synced {researchedTechnologyIds.Count} " +
            $"technologies from PlayerId={Player.PlayerId}.",
            this);
    }

    private void UpdateLocalPlayerReference()
    {
        if (!IsSpawned)
            return;

        FindReferences();

        if (playerManager == null || Player == null)
            return;

        // Локальним вважається NetworkPlayer,
        // власником якого є поточний клієнт.
        if (OwnerClientId != NetworkManager.LocalClientId)
            return;

        playerManager.SetLocalPlayer(Player);

        Debug.Log(
            $"[NetworkPlayer] Local player assigned. " +
            $"IsServer={IsServer}, " +
            $"LocalClientId={NetworkManager.LocalClientId}, " +
            $"OwnerClientId={OwnerClientId}, " +
            $"PlayerId={Player.PlayerId}"
        );
    }
}
