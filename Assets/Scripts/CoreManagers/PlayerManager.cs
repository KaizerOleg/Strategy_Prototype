
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private List<Player> players = new();

    public IReadOnlyList<Player> Players => players;

    public Player LocalPlayer { get; private set; }

    private void Awake()
    {
        InitializePlayers();
    }

    private void InitializePlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];

            if (player == null)
                continue;

            player.Initialize(i);
        }

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            LocalPlayer = null;
            return;
        }

        LocalPlayer = players.Count > 0 ? players[0] : null;
    }

    public void ResetPlayers()
    {
        foreach (Player player in players)
        {
            if (player == null)
                continue;

            player.ResetState();
        }
    }

    public Player GetPlayer(int playerId)
    {
        foreach (Player player in players)
        {
            if (player == null)
                continue;

            if (player.PlayerId == playerId)
                return player;
        }

        return null;
    }

    public void RegisterPlayer(Player player)
    {
        if (player == null || players.Contains(player))
            return;

        player.Initialize(players.Count);
        players.Add(player);
    }

    public void UnregisterPlayer(Player player)
    {
        if (player == null)
            return;

        players.Remove(player);

        if (LocalPlayer == player)
            LocalPlayer = null;
    }

    public void SetLocalPlayer(Player player)
    {
        if (player == null)
        {
            Debug.LogError(
                "[PlayerManager] Cannot set LocalPlayer: Player is null.");
            return;
        }

        if (!players.Contains(player))
        {
            Debug.LogError(
                "[PlayerManager] Cannot set LocalPlayer: Player is not registered.");
            return;
        }

        LocalPlayer = player;

        Debug.Log(
            $"[PlayerManager] LocalPlayer set to PlayerId={player.PlayerId}.");
    }
}
