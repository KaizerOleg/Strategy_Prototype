using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CityDatabase",
    menuName = "Game/City Database"
)]
public class CityDatabase : ScriptableObject
{
    [SerializeField]
    private List<CityData> cities = new();

    public IReadOnlyList<CityData> Cities => cities;
}