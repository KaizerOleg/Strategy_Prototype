using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkPlayer networkPlayerPrefab;
    [SerializeField] private NetworkPlayerManager networkPlayerManager;
    [SerializeField] private PlayerManager playerManager;

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback +=
            OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback +=
            OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -=
            OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback -=
            OnClientDisconnected;
    }

    private void OnClientConnected(
        ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        SpawnNetworkPlayer(clientId);
    }

    private void OnClientDisconnected(
        ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (networkPlayerManager == null)
            return;

        networkPlayerManager.UnregisterNetworkPlayer(
            clientId
        );
    }

    private void SpawnNetworkPlayer(
        ulong clientId)
    {
        if (networkPlayerPrefab == null)
        {
            Debug.LogError(
                "NetworkPlayer prefab is missing."
            );

            return;
        }

        if (networkPlayerManager == null)
        {
            Debug.LogError(
                "NetworkPlayerManager is missing."
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

        if (networkPlayerManager.GetNetworkPlayer(
                clientId) != null)
        {
            return;
        }

        int playerId =
            GetFreePlayerId();

        if (playerId < 0)
        {
            Debug.LogError(
                "No free PlayerId available."
            );

            return;
        }

        NetworkPlayer networkPlayer =
            Instantiate(
                networkPlayerPrefab
            );

        networkPlayer.NetworkObject.SpawnAsPlayerObject(
            clientId
        );

        networkPlayer.AssignPlayerId(
            playerId
        );

        networkPlayerManager.RegisterNetworkPlayer(
            clientId,
            networkPlayer
        );

        networkPlayerManager.LinkPlayer(
            networkPlayer
        );
    }

    private int GetFreePlayerId()
    {
        if (playerManager == null)
            return -1;

        for (
            int playerId = 0;
            playerId < playerManager.Players.Count;
            playerId++)
        {
            bool used = false;

            foreach (
                NetworkPlayer networkPlayer
                in networkPlayerManager.NetworkPlayers.Values)
            {
                if (networkPlayer == null)
                    continue;

                if (networkPlayer.PlayerId.Value == playerId)
                {
                    used = true;
                    break;
                }
            }

            if (!used)
                return playerId;
        }

        return -1;
    }
}