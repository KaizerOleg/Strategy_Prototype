using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "NewBuilding",
    menuName = "Game/Building Data"
)]
public class BuildingData : ScriptableObject
{
    [Header("Basic Info")]
    public string buildingName;

    [TextArea]
    public string description;

    [Header("Features")]
    [SerializeField] private Feature[] features;

    public IReadOnlyList<Feature> Features =>
        features;

    [Header("Technology")]
    public TechnologyData technologyRequirement;

    [Header("Cost")]
    [Min(0)]
    public int productionCost = 20;

    [Min(0)]
    public int goldCost = 0;

    [Header("Economy")]
    public Yield yieldBonus;

    [Header("Requirements")]
    public bool requiresCity = true;

    [Header("Allowed Terrain")]
    public bool canBuildOnAnyTerrain = true;

    public TerrainData[] allowedTerrains;

    [Header("Visual")]
    public Sprite sprite;
}