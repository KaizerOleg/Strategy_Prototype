using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCivilization",
    menuName = "Game/Civilization Data"
)]
public class CivilizationData : ScriptableObject
{
    [Header("Basic Info")]
    public string civilizationName;

    [TextArea]
    public string description;

    [Header("Databases")]
    public UnitDatabase unitDatabase;
    public BuildingDatabase buildingDatabase;
    public CityDatabase cityDatabase;
    public TechnologyDatabase technologyDatabase;
}