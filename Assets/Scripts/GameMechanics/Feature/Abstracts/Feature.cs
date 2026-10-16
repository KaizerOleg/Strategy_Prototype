using System.Collections.Generic;
using UnityEngine;

public enum FeatureType
{
    Passive,
    Active
}

public abstract class Feature : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string featureName;

    [TextArea]
    [SerializeField] private string description;

    [Header("Type")]
    [SerializeField] private FeatureType featureType;

    public string FeatureName => featureName;
    public string Description => description;
    public FeatureType Type => featureType;

    public bool IsPassive =>
        featureType == FeatureType.Passive;

    public bool IsActive =>
        featureType == FeatureType.Active;

    #region ACTIVE FEATURE

    public virtual bool RequiresTarget => false;

    public virtual bool CanActivate(
        FeatureContext context)
    {
        return false;
    }

    public virtual void Activate(
        FeatureContext context)
    {
    }
    #endregion

    #region PASSIVE FEATURE

    public virtual List<FeatureTarget> GetPassiveTargets(
        FeatureContext context)
    {
        return new List<FeatureTarget>();
    }

    public virtual bool CanApplyPassive(
        FeatureContext context)
    {
        return false;
    }

    public virtual ActiveEffect ApplyPassive(
        FeatureContext context)
    {
        return null;
    }
    #endregion
}