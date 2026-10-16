using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private Unit unit;
    private Pathfinding pathfinding;

    private void Awake()
    {
        unit = GetComponent<Unit>();

        if (unit == null)
        {
            Debug.LogError(
                "Unit component is missing."
            );
        }
    }

    public void InitializePathfinding(
        HexGrid grid)
    {
        if (grid == null)
        {
            Debug.LogError(
                "HexGrid is missing."
            );

            return;
        }

        pathfinding =
            new Pathfinding(grid);
    }

    public List<HexTile> GetAvailableTiles()
    {
        List<HexTile> result = new();

        if (unit == null)
        {
            Debug.LogError("[MovementRange] Unit is null.");
            return result;
        }

        if (unit.Data == null)
        {
            Debug.LogError("[MovementRange] Unit.Data is null.");
            return result;
        }

        if (unit.CurrentTile == null)
        {
            Debug.LogError("[MovementRange] CurrentTile is null.");
            return result;
        }

        if (unit.MovementPoints <= 0)
        {
            Debug.LogWarning(
                $"[MovementRange] MovementPoints = {unit.MovementPoints}"
            );
            return result;
        }

        if (pathfinding == null)
        {
            Debug.LogError("[MovementRange] Pathfinding is null.");
            return result;
        }

        result = pathfinding.GetReachableTiles(
            unit.CurrentTile,
            unit.MovementPoints
        );

        Debug.Log(
            $"[MovementRange] Unit: {unit.name}, " +
            $"Tile: {unit.CurrentTile.Coordinates}, " +
            $"Points: {unit.MovementPoints}, " +
            $"Reachable tiles: {result.Count}"
        );

        return result;
    }

    public bool CanMoveTo(
        HexTile targetTile)
    {
        if (targetTile == null)
            return false;

        if (unit == null)
            return false;

        if (unit.CurrentTile == null)
            return false;

        if (pathfinding == null)
            return false;

        if (!unit.CanMove())
            return false;

        if (targetTile.IsOccupied())
            return false;

        if (!pathfinding.TryGetMovementCost(
                unit.CurrentTile,
                targetTile,
                out int movementCost))
        {
            return false;
        }

        return unit.CanSpendMovement(
            movementCost
        );
    }

    public bool TryMoveTo(
        HexTile targetTile)
    {
        if (!CanMoveTo(targetTile))
            return false;

        if (!pathfinding.TryGetMovementCost(
                unit.CurrentTile,
                targetTile,
                out int movementCost))
        {
            return false;
        }

        unit.MoveTo(
            targetTile,
            movementCost
        );

        unit.HideMovementRange();

        return true;
    }
}