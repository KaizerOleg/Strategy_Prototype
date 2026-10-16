using UnityEngine;

[CreateAssetMenu(
    fileName = "NewMapData",
    menuName = "Game/Map Data"
)]
public class MapData : ScriptableObject
{
    [Header("Map Size")]
    [Min(1)]
    public int width = 40;

    [Min(1)]
    public int height = 40;

    [Header("Seed")]
    public bool useRandomSeed = true;

    public int seed = 12345;

    [Header("Height")]
    [Min(0.001f)]
    public float heightScale = 0.05f;

    [Header("Moisture")]
    [Min(0.001f)]
    public float moistureScale = 0.06f;

    [Header("Mountains")]
    [Range(0f, 1f)]
    public float mountainNoiseInfluence = 0.5f;
}