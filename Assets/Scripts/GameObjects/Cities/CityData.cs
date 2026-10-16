using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "NewCity",
    menuName = "Game/City Data"
)]
public class CityData : ScriptableObject
{
    [Header("Basic Info")]
    public string cityName;

    [TextArea]
    public string description;

    [Header("Features")]
    [SerializeField] private Feature[] features;

    public IReadOnlyList<Feature> Features =>
        features;

    [Header("Territory")]
    [Min(0)]
    public int territoryRadius = 2;

    [Header("Buildings")]
    [Min(0)]
    public int maxBuildingsPerTurn = 1;

    [Header("Unit Production")]
    [Min(0)]
    public int productionPerTurn = 10;

    [Header("Economy")]
    public Yield baseYield;

    [Header("Visual")]
    public Sprite sprite;
}