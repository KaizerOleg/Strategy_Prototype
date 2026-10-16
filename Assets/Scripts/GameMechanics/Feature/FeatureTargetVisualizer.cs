using System.Collections.Generic;
using UnityEngine;

public class FeatureTargetVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGrid grid;

    [Header("Visual")]
    [SerializeField] private Color targetColor = Color.red;

    private readonly List<HexTile> highlightedTiles = new();

    public void ShowAttackRange(Unit unit, int range)
    {
        Clear();

        if (unit == null)
            return;

        if (unit.CurrentTile == null)
            return;

        if (grid == null)
        {
            Debug.LogError(
                "HexGrid is missing in FeatureTargetVisualizer."
            );

            return;
        }

        if (range <= 0)
            return;

        foreach (HexTile tile in grid.GetAllTiles())
        {
            if (tile == null)
                continue;

            int distance =
                unit.CurrentTile.Coordinates.GetDistance(
                    tile.Coordinates
                );

            if (distance <= 0)
                continue;

            if (distance > range)
                continue;

            highlightedTiles.Add(tile);

            tile.View.SetFeatureTargetHighlight(
                true,
                targetColor
            );
        }
    }

    public void Clear()
    {
        foreach (HexTile tile in highlightedTiles)
        {
            if (tile == null || tile.View == null)
                continue;

            tile.View.SetFeatureTargetHighlight(
                false,
                Color.black
            );
        }

        highlightedTiles.Clear();
    }
}