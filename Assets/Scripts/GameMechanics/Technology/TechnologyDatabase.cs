using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TechnologyDatabase",
    menuName = "Game/Technology Database"
)]
public class TechnologyDatabase : ScriptableObject
{
    [SerializeField]
    private List<TechnologyData> technologies = new();

    public IReadOnlyList<TechnologyData> Technologies => technologies;
}