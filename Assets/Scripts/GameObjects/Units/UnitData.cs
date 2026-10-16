using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "NewUnit",
    menuName = "Game/Unit Data"
)]
public class UnitData : ScriptableObject
{
    public int UnitId;

    [Header("Basic Info")]
    public string unitName;

    [Header("Special Abilities")]
    public bool canFoundCity;

    [Header("Features")]
    [SerializeField] private Feature[] features;

    public IReadOnlyList<Feature> Features =>
        features;

    [TextArea]
    public string description;

    [Header("Technology")]
    public TechnologyData technologyRequirement;

    [Header("Cost")]
    [Min(1)]
    public int productionCost = 50;

    [Header("Movement")]
    public int movementRange = 3;

    [Header("Combat")]
    public int maxHealth = 100;
    public int attack = 10;
    public int defense = 5;

    [Header("Visual")]
    public Sprite sprite;
}
