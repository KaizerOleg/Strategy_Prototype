using System;
using UnityEngine;

public enum TerrainGenerationMode
{
    Base,
    Noise,
    Cluster,
    Ridge
}

public enum TerrainClusterShape
{
    Random,
    Round,
    Wide,
    Tall,
    Line,
    Irregular
}

[Serializable]
public class TerrainGenerationSettings
{
    [Header("Terrain")]
    public TerrainData terrain;

    public TerrainGenerationMode mode =
        TerrainGenerationMode.Cluster;

    [Header("Priority")]
    [Min(0)]
    public int priority = 0;

    [Header("Amount")]
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    [Header("Clusters")]
    [Min(1)]
    public int clusterCount = 5;

    [Min(1)]
    public int clusterSize = 10;

    [Header("Cluster Shape")]
    public TerrainClusterShape shape =
        TerrainClusterShape.Irregular;

    [Range(1, 20)]
    public int width = 5;

    [Range(1, 20)]
    public int height = 5;

    [Header("Noise")]
    [Min(0.001f)]
    public float noiseScale = 0.08f;

    [Range(0f, 1f)]
    public float noiseThreshold = 0.5f;

    [Range(0f, 1f)]
    public float noiseInfluence = 0.5f;

    [Header("Ridge")]
    [Range(0f, 1f)]
    public float ridgeRandomness = 0.25f;
}