
using Unity.Netcode;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private City cityPrefab;
    [SerializeField] private TerritorySystem territorySystem;

    public bool FoundCity(Unit unit, Player player, CityData cityData)
    {
        if (unit == null || player == null || unit.Data == null)
            return false;

        // У мережевій грі місто може заснувати лише сервер.
        NetworkManager networkManager = NetworkManager.Singleton;
        bool isNetworkGame =
            networkManager != null && networkManager.IsListening;

        if (isNetworkGame && !networkManager.IsServer)
            return false;

        if (!unit.Data.canFoundCity)
            return false;

        HexTile tile = unit.CurrentTile;

        if (tile == null || tile.HasCity() || tile.HasOwner())
            return false;

        if (cityPrefab == null)
        {
            Debug.LogError("[CityManager] City prefab is missing.");
            return false;
        }

        if (cityData == null)
        {
            Debug.LogError("[CityManager] CityData is missing.");
            return false;
        }

        CivilizationData civilization = player.GetCivilization();

        if (civilization == null ||
            civilization.cityDatabase == null)
        {
            Debug.LogError("[CityManager] CityDatabase is missing.");
            return false;
        }

        int cityTypeId = -1;
        var cities = civilization.cityDatabase.Cities;

        for (int i = 0; i < cities.Count; i++)
        {
            if (cities[i] == cityData)
            {
                cityTypeId = i;
                break;
            }
        }

        if (cityTypeId < 0)
        {
            Debug.LogError(
                $"[CityManager] CityData '{cityData.name}' " +
                "was not found in the owner's CityDatabase.");
            return false;
        }

        GameObject cityObject = Instantiate(
            cityPrefab.gameObject,
            tile.transform.position,
            Quaternion.identity);

        City city = cityObject.GetComponent<City>();

        if (city == null)
        {
            Debug.LogError("[CityManager] City component is missing.");
            Destroy(cityObject);
            return false;
        }

        city.Initialize(cityData, player, tile);

        if (isNetworkGame)
        {
            NetworkObject networkObject =
                cityObject.GetComponent<NetworkObject>();

            NetworkCity networkCity =
                cityObject.GetComponent<NetworkCity>();

            if (networkObject == null || networkCity == null)
            {
                Debug.LogError(
                    "[CityManager] City prefab must contain " +
                    "NetworkObject, NetworkCity and City.");

                Destroy(cityObject);
                return false;
            }

            // Спочатку реєструємо об'єкт у Netcode.
            networkObject.Spawn();

            // Тепер IsServer має правильне значення.
            // NetworkVariables синхронізуються з клієнтами.
            networkCity.InitializeNetwork(
                player,
                cityData,
                tile,
                cityTypeId);
        }

        if (territorySystem != null)
            territorySystem.GenerateTerritory(city);

        // Важливо: цей виклик має коректно видаляти мережевого юніта.
        unit.DestroyUnit();

        return true;
    }
}
