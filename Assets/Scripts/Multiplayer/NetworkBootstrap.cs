using Unity.Netcode;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField]
    private NetworkPlayerManager networkPlayerManager;

    private bool gameStarted;

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnServerStarted +=
            OnServerStarted;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnServerStarted -=
            OnServerStarted;
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError(
                "NetworkManager is missing."
            );

            return;
        }

        if (NetworkManager.Singleton.IsListening)
            return;

        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError(
                "NetworkManager is missing."
            );

            return;
        }

        if (NetworkManager.Singleton.IsListening)
            return;

        NetworkManager.Singleton.StartClient();
    }

    public void Shutdown()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsListening)
            return;

        NetworkManager.Singleton.Shutdown();

        gameStarted = false;
    }

    private void OnServerStarted()
    {
        StartGameOnServer();
    }

    private void StartGameOnServer()
    {
        if (gameStarted)
            return;

        if (turnManager == null)
        {
            Debug.LogError(
                "TurnManager is missing."
            );

            return;
        }

        gameStarted = true;

        if (networkPlayerManager == null)
        {
            networkPlayerManager =
                FindFirstObjectByType<NetworkPlayerManager>();
        }

        if (networkPlayerManager != null)
        {
            networkPlayerManager.ResetAllResearch();
        }
        else
        {
            Debug.LogError(
                "[NetworkBootstrap] NetworkPlayerManager is missing.");
        }

        turnManager.StartGame();
    }
}