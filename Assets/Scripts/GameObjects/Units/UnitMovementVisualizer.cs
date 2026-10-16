using System.Collections.Generic;
using UnityEngine;

public class UnitMovementVisualizer : MonoBehaviour
{
    private readonly List<HexTile> highlightedTiles = new();

    public void Show(List<HexTile> tiles)
    {
        Clear();

        foreach (HexTile tile in tiles)
        {
            tile.View.SetMovementHighlight(true);

            highlightedTiles.Add(tile);
        }
    }

    public void Clear()
    {
        foreach (HexTile tile in highlightedTiles)
        {
            if (tile != null && tile.View != null)
            {
                tile.View.SetMovementHighlight(false);
            }
        }

        highlightedTiles.Clear();
    }
}