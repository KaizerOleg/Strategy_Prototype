using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TerrainDatabase",
    menuName = "Game/Terrain Database"
)]
public class TerrainDatabase : ScriptableObject
{
    [SerializeField]
    private List<TerrainData> terrains = new();

    public IReadOnlyList<TerrainData> Terrains => terrains;
}