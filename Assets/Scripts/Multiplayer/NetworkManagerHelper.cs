using Unity.Netcode;

public static class NetworkManagerHelper
{
    public static bool IsNetworked
    {
        get
        {
            return
                NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsListening;
        }
    }
}