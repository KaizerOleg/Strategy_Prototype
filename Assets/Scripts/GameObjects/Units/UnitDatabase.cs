using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "UnitProductionDatabase",
    menuName = "Game/Unit Database"
)]
public class UnitDatabase : ScriptableObject
{
    [SerializeField]
    private List<UnitData> units = new();

    public IReadOnlyList<UnitData> Units => units;

    public UnitData GetUnit(int unitId)
    {
        foreach (UnitData unit in Units)
        {
            if (unit == null)
                continue;

            if (unit.UnitId == unitId)
                return unit;
        }

        return null;
    }
}