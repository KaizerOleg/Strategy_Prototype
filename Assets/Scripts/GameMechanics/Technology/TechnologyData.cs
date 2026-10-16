using UnityEngine;

[CreateAssetMenu(
    fileName = "NewTechnology",
    menuName = "Game/Technology Data"
)]
public class TechnologyData : ScriptableObject
{
    [Header("Basic Info")]
    public string technologyName;

    [TextArea]
    public string description;

    [Header("Tier")]
    [Range(1, 6)]
    public int tier = 1;

    [Header("Cost")]
    [Min(0)]
    public int researchCost = 50;

    [Header("Requirements")]
    public TechnologyData[] requiredTechnologies;

    [Header("Unlocks")]
    public UnitData[] unlockedUnits;
    public BuildingData[] unlockedBuildings;
    public CityData[] unlockedCities;

    [Header("Features")]
    public TechnologyFeatureData[] unlockedFeatures;

    [Header("Visual")]
    public Sprite icon;
}