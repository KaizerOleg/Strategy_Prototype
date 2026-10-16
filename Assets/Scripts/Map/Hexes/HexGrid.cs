using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private HexTile tilePrefab;

    [SerializeField] private float hexSize = 1f;

    private readonly Dictionary<HexCoordinates, HexTile> tiles = new();

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float MapMinX { get; private set; }
    public float MapMaxX { get; private set; }

    public float MapMinY { get; private set; }
    public float MapMaxY { get; private set; }

    public void Generate(int width, int height)
    {
        if (tilePrefab == null)
        {
            Debug.LogError(
                "HexTile prefab is missing."
            );

            return;
        }

        if (tiles.Count > 0)
        {
            Debug.LogWarning(
                "Grid has already been generated."
            );

            return;
        }

        Width = width;
        Height = height;

        for (int r = 0; r < height; r++)
        {
            for (int q = 0; q < width; q++)
            {
                HexCoordinates coordinates =
                    new HexCoordinates(q, r);

                Vector3 position =
                    HexToWorldPosition(coordinates);

                HexTile tile =
                    Instantiate(
                        tilePrefab,
                        position,
                        Quaternion.identity,
                        transform
                    );

                tile.Initialize(
                    coordinates
                );

                tiles.Add(
                    coordinates,
                    tile
                );
            }
        }
        MapMinX = float.MaxValue;
        MapMaxX = float.MinValue;
        MapMinY = float.MaxValue;
        MapMaxY = float.MinValue;

        foreach (HexTile tile in tiles.Values)
        {
            Vector3 position = tile.transform.position;

            if (position.x < MapMinX)
                MapMinX = position.x;

            if (position.x > MapMaxX)
                MapMaxX = position.x;

            if (position.y < MapMinY)
                MapMinY = position.y;

            if (position.y > MapMaxY)
                MapMaxY = position.y;
        }
    }

    private Vector3 HexToWorldPosition(HexCoordinates coordinates)
    {
        float x =
            hexSize *
            1.5f *
            coordinates.Q;

        float y =
            hexSize *
            Mathf.Sqrt(3f) *
            (
                coordinates.R +
                coordinates.Q * 0.5f
            );

        return new Vector3(
            x,
            y,
            0f
        );
    }

    public HexTile GetTile(HexCoordinates coordinates)
    {
        tiles.TryGetValue(
            coordinates,
            out HexTile tile
        );

        return tile;
    }

    public List<HexTile> GetNeighbors(HexCoordinates coordinates)
    {
        List<HexTile> neighbors = new();

        foreach (
            HexCoordinates neighborCoordinates
            in coordinates.GetNeighbors())
        {
            HexTile tile =
                GetTile(
                    neighborCoordinates
                );

            if (tile != null)
            {
                neighbors.Add(tile);
            }
        }

        return neighbors;
    }

    public List<HexTile> GetAllTiles()
    {
        return new List<HexTile>(
            tiles.Values
        );
    }

    public void ClearGrid()
    {
        foreach (HexTile tile in tiles.Values)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }

        tiles.Clear();

        Width = 0;
        Height = 0;
    }

    public void ClearTileOwners()
    {
        foreach (HexTile tile in tiles.Values)
        {
            if (tile == null)
                continue;

            tile.SetOwner(null);
        }

        Debug.Log("[HexGrid] Cleared all tile owners.");
    }
}