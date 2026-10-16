
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFoundCityFeature",
    menuName = "Game/Features/Found City")]
public class FoundCityFeature : Feature
{
    public override bool CanActivate(FeatureContext context)
    {
        if (context == null ||
            context.Unit == null ||
            context.Player == null)
        {
            return false;
        }

        Unit unit = context.Unit;
        Player player = context.Player;

        if (unit.Owner != player)
            return false;

        if (unit.Data == null || !unit.Data.canFoundCity)
            return false;

        HexTile tile = unit.CurrentTile;

        if (tile == null || tile.HasCity() || tile.HasOwner())
            return false;

        CivilizationData civilization = player.GetCivilization();

        if (civilization == null ||
            civilization.cityDatabase == null ||
            civilization.cityDatabase.Cities.Count == 0)
        {
            return false;
        }

        return civilization.cityDatabase.Cities[0] != null;
    }

    public override void Activate(FeatureContext context)
    {
        if (!CanActivate(context))
            return;

        CityManager cityManager =
            Object.FindFirstObjectByType<CityManager>();

        if (cityManager == null)
        {
            Debug.LogError("[FoundCityFeature] CityManager not found.");
            return;
        }

        Player player = context.Player;

        CityData cityData =
            player.GetCivilization().cityDatabase.Cities[0];

        cityManager.FoundCity(
            context.Unit,
            player,
            cityData);
    }
}
