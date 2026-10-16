using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FeatureSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CityManager cityManager;
    [SerializeField] private EffectSystem effectSystem;

    public EffectSystem EffectSystem =>
        effectSystem;

    public event Action OnFeatureActivated;

    public bool CanActivate(
        Feature feature,
        FeatureContext context)
    {
        if (feature == null)
            return false;

        if (!feature.IsActive)
            return false;

        if (context == null)
            return false;

        return feature.CanActivate(context);
    }

    public bool Activate(
        Feature feature,
        FeatureContext context)
    {
        if (NetworkManager.Singleton != null &&
        NetworkManager.Singleton.IsListening &&
        !NetworkManager.Singleton.IsServer)
        {
            return false;
        }

        if (!CanActivate(feature, context))
            return false;

        feature.Activate(context);

        OnFeatureActivated?.Invoke();

        return true;
    }

    public void ProcessPassiveFeatures(Player player)
    {
        if (NetworkManager.Singleton != null &&
        NetworkManager.Singleton.IsListening &&
        !NetworkManager.Singleton.IsServer)
        {
            return;
        }
        if (player == null)
            return;

        if (effectSystem == null)
        {
            Debug.LogError(
                "EffectSystem is missing."
            );

            return;
        }

        HashSet<ActiveEffect> validPassiveEffects =
            new HashSet<ActiveEffect>();

        ProcessPlayerUnits(
            player,
            validPassiveEffects
        );

        ProcessPlayerCities(
            player,
            validPassiveEffects
        );

        effectSystem.RemoveStalePassiveEffects(
            player,
            validPassiveEffects
        );
    }

    private void ProcessPlayerUnits(
        Player player,
        HashSet<ActiveEffect> validPassiveEffects)
    {
        List<Unit> units =
            new List<Unit>(player.Units);

        foreach (Unit unit in units)
        {
            if (unit == null)
                continue;

            if (unit.Data == null)
                continue;

            ProcessFeatures(
                unit.Data.Features,
                unit,
                null,
                null,
                player,
                validPassiveEffects
            );
        }
    }

    private void ProcessPlayerCities(
        Player player,
        HashSet<ActiveEffect> validPassiveEffects)
    {
        List<City> cities =
            new List<City>(player.Cities);

        foreach (City city in cities)
        {
            if (city == null)
                continue;

            if (city.Data == null)
                continue;

            ProcessFeatures(
                city.Data.Features,
                null,
                city,
                null,
                player,
                validPassiveEffects
            );

            ProcessCityBuildings(
                city,
                player,
                validPassiveEffects
            );
        }
    }

    private void ProcessCityBuildings(
        City city,
        Player player,
        HashSet<ActiveEffect> validPassiveEffects)
    {
        List<Building> buildings =
            new List<Building>(city.Buildings);

        foreach (Building building in buildings)
        {
            if (building == null)
                continue;

            if (building.Data == null)
                continue;

            ProcessFeatures(
                building.Data.Features,
                null,
                null,
                building,
                player,
                validPassiveEffects
            );
        }
    }

    private void ProcessFeatures(
        IReadOnlyList<Feature> features,
        Unit unit,
        City city,
        Building building,
        Player player,
        HashSet<ActiveEffect> validPassiveEffects)
    {
        if (features == null)
            return;

        foreach (Feature feature in features)
        {
            if (feature == null)
                continue;

            if (!feature.IsPassive)
                continue;

            FeatureContext baseContext =
                new FeatureContext(
                    unit: unit,
                    city: city,
                    building: building,
                    player: player,
                    effectSystem: effectSystem
                );

            List<FeatureTarget> targets =
                feature.GetPassiveTargets(
                    baseContext
                );

            if (targets == null)
                continue;

            foreach (FeatureTarget target in targets)
            {
                if (target == null)
                    continue;

                FeatureContext context =
                    new FeatureContext(
                        unit: unit,
                        city: city,
                        building: building,
                        player: player,
                        target: target,
                        effectSystem: effectSystem
                    );

                if (!feature.CanApplyPassive(context))
                    continue;

                ActiveEffect effect =
                    feature.ApplyPassive(context);

                if (effect != null)
                {
                    validPassiveEffects.Add(effect);
                }
            }
        }
    }
}