using System.Collections.Generic;

public class Pathfinding
{
    private readonly HexGrid grid;

    public Pathfinding(HexGrid grid)
    {
        this.grid = grid;
    }

    public List<HexTile> GetReachableTiles(
        HexTile startTile,
        int movementPoints)
    {
        List<HexTile> result = new();

        if (startTile == null ||
            movementPoints <= 0)
        {
            return result;
        }

        Dictionary<HexTile, int> costs = new();
        List<HexTile> open = new();

        costs[startTile] = 0;
        open.Add(startTile);

        while (open.Count > 0)
        {
            HexTile current =
                GetLowestCostTile(
                    open,
                    costs
                );

            open.Remove(current);

            int currentCost =
                costs[current];

            foreach (HexTile neighbor in
                     grid.GetNeighbors(
                         current.Coordinates))
            {
                if (!CanEnterTile(neighbor))
                    continue;

                int movementCost =
                    neighbor.Terrain.movementCost;

                int newCost =
                    currentCost +
                    movementCost;

                if (newCost > movementPoints)
                    continue;

                if (!costs.ContainsKey(neighbor) ||
                    newCost < costs[neighbor])
                {
                    costs[neighbor] = newCost;

                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }

        foreach (KeyValuePair<HexTile, int> pair
                 in costs)
        {
            if (pair.Key == startTile)
                continue;

            result.Add(pair.Key);
        }

        return result;
    }

    public bool TryGetMovementCost(
        HexTile startTile,
        HexTile targetTile,
        out int movementCost)
    {
        movementCost = 0;

        if (startTile == null ||
            targetTile == null)
        {
            return false;
        }

        Dictionary<HexTile, int> costs = new();
        List<HexTile> open = new();

        costs[startTile] = 0;
        open.Add(startTile);

        while (open.Count > 0)
        {
            HexTile current =
                GetLowestCostTile(
                    open,
                    costs
                );

            open.Remove(current);

            if (current == targetTile)
            {
                movementCost =
                    costs[current];

                return true;
            }

            int currentCost =
                costs[current];

            foreach (HexTile neighbor in
                     grid.GetNeighbors(
                         current.Coordinates))
            {
                if (!CanEnterTile(neighbor))
                    continue;

                int tileCost =
                    neighbor.Terrain.movementCost;

                int newCost =
                    currentCost +
                    tileCost;

                if (!costs.ContainsKey(neighbor) ||
                    newCost < costs[neighbor])
                {
                    costs[neighbor] = newCost;

                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }

        return false;
    }

    private bool CanEnterTile(
        HexTile tile)
    {
        if (tile == null)
            return false;

        if (tile.Terrain == null)
            return false;

        if (!tile.Terrain.isWalkable)
            return false;

        if (tile.IsOccupied())
            return false;

        return true;
    }

    private HexTile GetLowestCostTile(
        List<HexTile> tiles,
        Dictionary<HexTile, int> costs)
    {
        HexTile lowest = tiles[0];

        foreach (HexTile tile in tiles)
        {
            if (costs[tile] < costs[lowest])
                lowest = tile;
        }

        return lowest;
    }
}
