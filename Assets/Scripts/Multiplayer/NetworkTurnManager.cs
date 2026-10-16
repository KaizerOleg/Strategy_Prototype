using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTurnManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private NetworkPlayerManager networkPlayerManager;

    public NetworkVariable<int> CurrentTurn =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<int> CurrentPlayerId =
        new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public event Action<int> OnNetworkTurnChanged;
    public event Action<int> OnNetworkPlayerChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CurrentTurn.OnValueChanged +=
            HandleTurnChanged;

        CurrentPlayerId.OnValueChanged +=
            HandlePlayerChanged;

        if (IsServer)
        {
            SyncTurnState();
        }
        else
        {
            ApplyNetworkState();
        }
    }

    public override void OnNetworkDespawn()
    {
        CurrentTurn.OnValueChanged -=
            HandleTurnChanged;

        CurrentPlayerId.OnValueChanged -=
            HandlePlayerChanged;

        base.OnNetworkDespawn();
    }

    public void RequestEndTurn()
    {
        if (!IsClient)
            return;

        RequestEndTurnRpc();
    }

    [Rpc(SendTo.Server)]
    private void RequestEndTurnRpc(
        RpcParams rpcParams = default)
    {
        ulong clientId =
            rpcParams.Receive.SenderClientId;

        if (!CanClientEndTurn(clientId))
            return;

        turnManager.EndTurn();

        SyncTurnState();
    }

    private bool CanClientEndTurn(
        ulong clientId)
    {
        if (turnManager == null)
            return false;

        if (networkPlayerManager == null)
            return false;

        Player player =
            networkPlayerManager.GetPlayer(
                clientId
            );

        if (player == null)
            return false;

        return
            turnManager.CurrentPlayer != null &&
            turnManager.CurrentPlayer == player;
    }

    private void SyncTurnState()
    {
        if (!IsServer)
            return;

        if (turnManager == null)
            return;

        CurrentTurn.Value =
            turnManager.CurrentTurn;

        if (turnManager.CurrentPlayer == null)
        {
            CurrentPlayerId.Value = -1;
            return;
        }

        CurrentPlayerId.Value =
            turnManager.CurrentPlayer.PlayerId;
    }

    private void ApplyNetworkState()
    {
        if (turnManager == null)
            return;

        if (CurrentTurn.Value <= 0)
            return;

        if (CurrentPlayerId.Value < 0)
            return;

        turnManager.ApplyNetworkState(
            CurrentTurn.Value,
            CurrentPlayerId.Value
        );
    }

    private void HandleTurnChanged(
        int previousValue,
        int newValue)
    {
        if (!IsServer)
        {
            ApplyNetworkState();
        }

        OnNetworkTurnChanged?.Invoke(
            newValue
        );
    }

    private void HandlePlayerChanged(
        int previousValue,
        int newValue)
    {
        if (!IsServer)
        {
            ApplyNetworkState();
        }

        OnNetworkPlayerChanged?.Invoke(
            newValue
        );
    }
}