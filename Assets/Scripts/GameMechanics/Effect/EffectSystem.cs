using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    public event Action<ActiveEffect> OnEffectApplied;
    public event Action<ActiveEffect> OnEffectRemoved;

    #region NORMAL EFFECTS

    public bool ApplyEffect(
        EffectData effectData,
        int duration,
        EffectContext context)
    {
        if (effectData == null)
            return false;

        if (context == null)
            return false;

        if (context.Target == null)
            return false;

        EffectContainer container =
            context.Target.EffectContainer;

        if (container == null)
            return false;

        if (effectData.DurationType ==
            EffectDurationType.Turns &&
            duration <= 0)
        {
            return false;
        }

        ActiveEffect existingEffect =
            container.GetEffect(effectData);

        if (existingEffect != null)
        {
            return HandleExistingEffect(
                existingEffect,
                duration,
                effectData.StackMode
            );
        }

        ActiveEffect effect =
            new ActiveEffect(
                effectData,
                duration,
                context
            );

        container.AddEffect(effect);

        OnEffectApplied?.Invoke(effect);

        return true;
    }
    #endregion

    #region PASSIVE EFFECTS

    public ActiveEffect ApplyPassiveEffect(
    EffectData effectData,
    FeatureContext featureContext,
    Feature sourceFeature)
    {
        if (effectData == null)
            return null;

        if (featureContext == null)
            return null;

        if (featureContext.Target == null)
            return null;

        if (sourceFeature == null)
            return null;

        EffectTarget effectTarget =
            new EffectTarget(
                unit: featureContext.Target.Unit,
                city: featureContext.Target.City,
                building: featureContext.Target.Building
            );

        EffectContainer container =
            effectTarget.EffectContainer;

        if (container == null)
            return null;

        ActiveEffect existingEffect =
            container.GetPassiveEffect(
                effectData,
                sourceFeature,
                featureContext.Unit,
                featureContext.City,
                featureContext.Building
            );

        if (existingEffect != null)
        {
            return existingEffect;
        }

        EffectContext effectContext =
            new EffectContext(
                target: effectTarget,
                sourceUnit: featureContext.Unit,
                sourceCity: featureContext.City,
                sourceBuilding: featureContext.Building,
                sourcePlayer: featureContext.Player
            );

        ActiveEffect effect =
            new ActiveEffect(
                effectData,
                0,
                effectContext,
                sourceFeature,
                true
            );

        Debug.Log(
            $"Applying passive effect: " +
            $"{effectData.name} to " +
            $"{effectTarget.Unit.name}"
        );

        container.AddEffect(effect);

        OnEffectApplied?.Invoke(effect);

        return effect;
    }
    #endregion

    #region STACKING

    private bool HandleExistingEffect(
        ActiveEffect effect,
        int duration,
        EffectStackMode stackMode)
    {
        switch (stackMode)
        {
            case EffectStackMode.AllowStacks:

                effect.AddStack();

                return true;

            case EffectStackMode.RefreshDuration:

                effect.RefreshDuration(duration);

                return true;

            case EffectStackMode.IgnoreIfAlreadyPresent:

                return false;

            default:

                return false;
        }
    }
    #endregion

    #region REMOVE

    public void RemoveEffect(
        EffectContainer container,
        EffectData effectData)
    {
        if (container == null)
            return;

        ActiveEffect effect =
            container.GetEffect(effectData);

        if (effect == null)
            return;

        container.RemoveEffect(effect);

        OnEffectRemoved?.Invoke(effect);
    }

    public void RemoveStalePassiveEffects(
    Player player,
    HashSet<ActiveEffect> validPassiveEffects)
    {
        if (player == null)
            return;

        if (validPassiveEffects == null)
            return;

        List<Unit> units =
            new List<Unit>(player.Units);

        foreach (Unit unit in units)
        {
            if (unit == null ||
                unit.Effects == null)
            {
                continue;
            }

            RemoveStaleEffectsFromContainer(
                unit.Effects,
                validPassiveEffects
            );
        }

        List<City> cities =
            new List<City>(player.Cities);

        foreach (City city in cities)
        {
            if (city == null)
                continue;

            if (city.Effects != null)
            {
                RemoveStaleEffectsFromContainer(
                    city.Effects,
                    validPassiveEffects
                );
            }

            List<Building> buildings =
                new List<Building>(city.Buildings);

            foreach (Building building in buildings)
            {
                if (building == null ||
                    building.Effects == null)
                {
                    continue;
                }

                RemoveStaleEffectsFromContainer(
                    building.Effects,
                    validPassiveEffects
                );
            }
        }
    }

    private void RemoveStaleEffectsFromContainer(
        EffectContainer container,
        HashSet<ActiveEffect> validPassiveEffects)
    {
        List<ActiveEffect> effects =
            new List<ActiveEffect>(
                container.Effects
            );

        foreach (ActiveEffect effect in effects)
        {
            if (effect == null)
                continue;

            if (!effect.IsPassive)
                continue;

            if (validPassiveEffects.Contains(effect))
                continue;

            container.RemoveEffect(effect);

            OnEffectRemoved?.Invoke(effect);
        }
    }
    #endregion

    #region TURN START

    public void ProcessTurnStarted(
        Player player)
    {
        if (player == null)
            return;

        List<Unit> units =
            new List<Unit>(player.Units);

        foreach (Unit unit in units)
        {
            if (unit == null ||
                unit.Effects == null)
            {
                continue;
            }

            unit.Effects.ProcessTurnStarted();
        }

        List<City> cities =
            new List<City>(player.Cities);

        foreach (City city in cities)
        {
            if (city == null ||
                city.Effects == null)
            {
                continue;
            }

            city.Effects.ProcessTurnStarted();

            List<Building> buildings =
                new List<Building>(city.Buildings);

            foreach (Building building in buildings)
            {
                if (building == null ||
                    building.Effects == null)
                {
                    continue;
                }

                building.Effects.ProcessTurnStarted();
            }
        }
    }
    #endregion

    #region TURN END

    public void ProcessTurnEnded(
        Player player)
    {
        if (player == null)
            return;

        List<Unit> units =
            new List<Unit>(player.Units);

        foreach (Unit unit in units)
        {
            if (unit == null ||
                unit.Effects == null)
            {
                continue;
            }

            unit.Effects.ProcessTurnEnded();
        }

        List<City> cities =
            new List<City>(player.Cities);

        foreach (City city in cities)
        {
            if (city == null ||
                city.Effects == null)
            {
                continue;
            }

            city.Effects.ProcessTurnEnded();

            List<Building> buildings =
                new List<Building>(city.Buildings);

            foreach (Building building in buildings)
            {
                if (building == null ||
                    building.Effects == null)
                {
                    continue;
                }

                building.Effects.ProcessTurnEnded();
            }
        }
    }
    #endregion
}