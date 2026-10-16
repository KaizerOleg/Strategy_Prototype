using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;

    private readonly Dictionary<
        ulong,
        NetworkPlayer
    > networkPlayers = new();

    public IReadOnlyDictionary<
        ulong,
        NetworkPlayer
    > NetworkPlayers =>
        networkPlayers;

    public void RegisterNetworkPlayer(
        ulong clientId,
        NetworkPlayer networkPlayer)
    {
        if (networkPlayer == null)
            return;

        networkPlayers[clientId] =
            networkPlayer;

        Debug.Log(
            $"Registered NetworkPlayer " +
            $"for Client {clientId}."
        );
    }
    public void ResetAllResearch()
    {
        var playersToReset = new HashSet<NetworkPlayer>(
            networkPlayers.Values);

        NetworkPlayer[] scenePlayers =
            FindObjectsByType<NetworkPlayer>(
                FindObjectsSortMode.None);

        foreach (NetworkPlayer networkPlayer in scenePlayers)
        {
            if (networkPlayer != null)
                playersToReset.Add(networkPlayer);
        }

        foreach (NetworkPlayer networkPlayer in playersToReset)
        {
            if (networkPlayer != null)
                networkPlayer.ResetResearchState();
        }
    }


    public void UnregisterNetworkPlayer(
        ulong clientId)
    {
        if (!networkPlayers.Remove(clientId))
            return;

        Debug.Log(
            $"Unregistered NetworkPlayer " +
            $"for Client {clientId}."
        );
    }

    public NetworkPlayer GetNetworkPlayer(
        ulong clientId)
    {
        networkPlayers.TryGetValue(
            clientId,
            out NetworkPlayer networkPlayer
        );

        return networkPlayer;
    }

    public Player GetPlayer(
        ulong clientId)
    {
        NetworkPlayer networkPlayer =
            GetNetworkPlayer(clientId);

        if (networkPlayer == null)
            return null;

        return networkPlayer.Player;
    }

    public void LinkPlayer(
        NetworkPlayer networkPlayer)
    {
        if (networkPlayer == null)
            return;

        if (!networkPlayer.IsAssigned)
        {
            Debug.LogWarning(
                "Cannot link Player. " +
                "NetworkPlayer has no PlayerId."
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

        Player player =
            playerManager.GetPlayer(
                networkPlayer.PlayerId.Value
            );

        if (player == null)
        {
            Debug.LogError(
                $"Player " +
                $"{networkPlayer.PlayerId.Value} " +
                $"was not found."
            );

            return;
        }

        networkPlayer.AssignPlayer(
            player
        );
    }

    public void Clear()
    {
        networkPlayers.Clear();
    }
}