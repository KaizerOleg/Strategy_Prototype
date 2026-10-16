using UnityEngine;

[CreateAssetMenu(
    fileName = "NewTerrain",
    menuName = "Game/Terrain Data"
)]
public class TerrainData : ScriptableObject
{
    [Header("Basic Info")]
    public string terrainName;

    [TextArea]
    public string description;

    [Header("Movement")]
    public int movementCost = 1;
    public bool isWalkable = true;

    [Header("Combat")]
    public int defenseBonus;

    [Header("Resources")]
    public int food;
    public int production;
    public int gold;
    public int science;

    [Header("Visual")]
    public Color color = Color.white;
    public Sprite sprite;
    public Sprite icon;

    [Header("Generation")]
    public bool canGenerate = true;

    public bool isFallback;

    [Range(0f, 1f)]
    public float minHeight;

    [Range(0f, 1f)]
    public float maxHeight = 1f;

    [Range(0f, 1f)]
    public float minMoisture;

    [Range(0f, 1f)]
    public float maxMoisture = 1f;

    [Min(0)]
    public int priority;

    [Range(0f, 1f)]
    public float spawnChance = 1f;
}