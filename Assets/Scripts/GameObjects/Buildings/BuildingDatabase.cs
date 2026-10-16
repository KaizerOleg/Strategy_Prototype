using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BuildingDatabase",
    menuName = "Game/Building Database"
)]
public class BuildingDatabase : ScriptableObject
{
    [SerializeField]
    private List<BuildingData> buildings = new();

    public IReadOnlyList<BuildingData> Buildings => buildings;
}