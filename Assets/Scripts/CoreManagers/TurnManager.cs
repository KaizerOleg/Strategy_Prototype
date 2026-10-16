
using System;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int CurrentTurn { get; private set; }
    public Player CurrentPlayer { get; private set; }

    public event Action<int> OnTurnStarted;
    public event Action<int> OnTurnEnded;
    public event Action<Player> OnPlayerTurnStarted;

    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private EconomySystem economySystem;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private EffectSystem effectSystem;
    [SerializeField] private FeatureSystem featureSystem;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private HexGrid grid;

    private int currentPlayerIndex;

    public Player LocalPlayer =>
        playerManager != null ? playerManager.LocalPlayer : null;

    public bool IsLocalPlayerTurn =>
        LocalPlayer != null &&
        CurrentPlayer == LocalPlayer;

    public void StartGame()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (playerManager == null)
        {
            Debug.LogError("[TurnManager] PlayerManager is missing.");
            return;
        }

        if (mapGenerator == null)
        {
            Debug.LogError("[TurnManager] MapGenerator is missing.");
            return;
        }

        if (economySystem == null)
        {
            Debug.LogError("[TurnManager] EconomySystem is missing.");
            return;
        }

        if (unitManager == null)
        {
            Debug.LogError("[TurnManager] UnitManager is missing.");
            return;
        }

        if (playerManager.Players.Count == 0)
        {
            Debug.LogError("[TurnManager] No players registered.");
            return;
        }

        playerManager.ResetPlayers();

        if (grid == null)
            grid = FindFirstObjectByType<HexGrid>();

        if (grid == null)
        {
            Debug.LogError("[TurnManager] HexGrid is missing.");
            return;
        }

        grid.ClearTileOwners();

        CurrentTurn = 1;
        currentPlayerIndex = 0;
        CurrentPlayer = null;

        unitManager.SpawnStartingUnits();

        StartCurrentPlayerTurn();
    }

    private void StartCurrentPlayerTurn()
    {
        if (playerManager == null ||
            currentPlayerIndex < 0 ||
            currentPlayerIndex >= playerManager.Players.Count)
        {
            Debug.LogError("[TurnManager] Invalid player index.");
            return;
        }

        CurrentPlayer = playerManager.Players[currentPlayerIndex];

        if (CurrentPlayer == null)
        {
            Debug.LogError("[TurnManager] CurrentPlayer is null.");
            return;
        }

        ProcessPlayerEffects();
        ResetPlayerUnits();
        ResetPlayerCities();
        ProcessPlayerEconomy();

        OnTurnStarted?.Invoke(CurrentTurn);
        OnPlayerTurnStarted?.Invoke(CurrentPlayer);

        Debug.Log(
            $"[TurnManager] Turn {CurrentTurn}, " +
            $"active PlayerId={CurrentPlayer.PlayerId}, " +
            $"local PlayerId={LocalPlayer?.PlayerId.ToString() ?? "none"}");
    }

    public void EndTurn()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning(
                "[TurnManager] Only the server can end a turn.");
            return;
        }

        if (CurrentPlayer == null)
        {
            Debug.LogError(
                "[TurnManager] Cannot end turn: CurrentPlayer is null.");
            return;
        }

        if (featureSystem != null)
            featureSystem.ProcessPassiveFeatures(CurrentPlayer);

        Debug.Log(
            $"[TurnManager] Ending turn for Player {CurrentPlayer.PlayerId}");

        OnTurnEnded?.Invoke(CurrentTurn);

        currentPlayerIndex++;

        if (currentPlayerIndex >= playerManager.Players.Count)
        {
            currentPlayerIndex = 0;
            CurrentTurn++;
        }

        StartCurrentPlayerTurn();
    }

    public void ApplyNetworkState(int turn, int playerId)
    {
        if (playerManager == null)
            return;

        Player player = playerManager.GetPlayer(playerId);

        if (player == null)
        {
            Debug.LogWarning(
                $"[TurnManager] Player {playerId} was not found.");
            return;
        }

        CurrentTurn = turn;
        CurrentPlayer = player;

        currentPlayerIndex = -1;

        for (int i = 0; i < playerManager.Players.Count; i++)
        {
            if (playerManager.Players[i] == player)
            {
                currentPlayerIndex = i;
                break;
            }
        }

        if (currentPlayerIndex < 0)
        {
            Debug.LogError(
                $"[TurnManager] Player {playerId} is not in the player list.");
            return;
        }

        OnTurnStarted?.Invoke(CurrentTurn);
        OnPlayerTurnStarted?.Invoke(CurrentPlayer);

        Debug.Log(
            $"[TurnManager] Network state applied. " +
            $"Turn={CurrentTurn}, " +
            $"active PlayerId={CurrentPlayer.PlayerId}, " +
            $"local PlayerId={LocalPlayer?.PlayerId.ToString() ?? "none"}");
    }

    private void ProcessPlayerEffects()
    {
        if (effectSystem != null)
            effectSystem.ProcessTurnStarted(CurrentPlayer);
    }

    private void ResetPlayerUnits()
    {
        if (CurrentPlayer == null)
            return;

        foreach (Unit unit in CurrentPlayer.Units)
        {
            if (unit == null)
                continue;

            unit.ResetMovement();

            NetworkUnit networkUnit =
                unit.GetComponent<NetworkUnit>();

            if (networkUnit != null &&
                networkUnit.IsSpawned &&
                NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsServer)
            {
                networkUnit.ServerSyncState();
            }
        }
    }

    private void ResetPlayerCities()
    {
        if (CurrentPlayer == null)
            return;

        foreach (City city in CurrentPlayer.Cities)
        {
            if (city != null)
                city.ResetTurn();
        }
    }

    private void ProcessPlayerEconomy()
    {
        if (CurrentPlayer == null)
            return;

        foreach (City city in CurrentPlayer.Cities)
        {
            if (city != null && city.Data != null)
                city.AddProduction(city.Data.productionPerTurn);
        }

        economySystem.CollectIncome(CurrentPlayer);
    }
}
