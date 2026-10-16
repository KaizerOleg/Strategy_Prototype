using System.Collections.Generic;
using UnityEngine;

public class EffectContainer : MonoBehaviour
{
    private readonly List<ActiveEffect> effects = new();

    public IReadOnlyList<ActiveEffect> Effects =>
        effects;

    public void AddEffect(
        ActiveEffect effect)
    {
        if (effect == null)
            return;

        if (effect.Data == null)
            return;

        effects.Add(effect);

        effect.Data.OnApplied(effect);
    }

    public void RemoveEffect(
        ActiveEffect effect)
    {
        if (effect == null)
            return;

        if (!effects.Remove(effect))
            return;

        effect.Data.OnRemoved(effect);
    }

    public ActiveEffect GetEffect(
        EffectData effectData)
    {
        if (effectData == null)
            return null;

        foreach (ActiveEffect effect in effects)
        {
            if (effect == null)
                continue;

            if (effect.Data == effectData)
                return effect;
        }

        return null;
    }

    public ActiveEffect GetPassiveEffect(
        EffectData effectData,
        Feature sourceFeature,
        Unit sourceUnit,
        City sourceCity,
        Building sourceBuilding)
    {
        if (effectData == null)
            return null;

        if (sourceFeature == null)
            return null;

        foreach (ActiveEffect effect in effects)
        {
            if (effect == null)
                continue;

            if (!effect.IsPassive)
                continue;

            if (effect.Data != effectData)
                continue;

            if (effect.SourceFeature != sourceFeature)
                continue;

            if (effect.SourceUnit != sourceUnit)
                continue;

            if (effect.SourceCity != sourceCity)
                continue;

            if (effect.SourceBuilding != sourceBuilding)
                continue;

            return effect;
        }

        return null;
    }

    public bool HasEffect(
        EffectData effectData)
    {
        return GetEffect(effectData) != null;
    }

    public void ProcessTurnStarted()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = effects[i];

            if (effect == null ||
                effect.Data == null)
            {
                effects.RemoveAt(i);
                continue;
            }

            effect.Data.OnTurnStarted(effect);

            effect.DecreaseDuration();

            if (effect.HasExpired())
            {
                RemoveEffect(effect);
            }
        }
    }

    public void ProcessTurnEnded()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = effects[i];

            if (effect == null ||
                effect.Data == null)
            {
                effects.RemoveAt(i);
                continue;
            }

            effect.Data.OnTurnEnded(effect);
        }
    }

    public void ClearEffects()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = effects[i];

            if (effect != null &&
                effect.Data != null)
            {
                effect.Data.OnRemoved(effect);
            }
        }

        effects.Clear();
    }
}