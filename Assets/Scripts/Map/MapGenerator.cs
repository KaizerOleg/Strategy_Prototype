using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGrid grid;
    [SerializeField] private MapData mapData;
    [SerializeField] private TerrainDatabase terrainDatabase;

    private readonly List<HexTile> allTiles = new();

    private System.Random random;

    private int currentSeed;

    public void GenerateMap()
    {
        if (!ValidateReferences())
            return;

        InitializeSeed();

        grid.Generate(
            mapData.width,
            mapData.height
        );

        allTiles.Clear();

        allTiles.AddRange(
            grid.GetAllTiles()
        );

        GenerateHeightMap();
        GenerateMoistureMap();
        GenerateTerrains();
        FillEmptyTiles();

        Debug.Log(
            $"Map generated. Seed: {currentSeed}"
        );
    }

    private bool ValidateReferences()
    {
        if (grid == null)
        {
            Debug.LogError(
                "HexGrid is missing."
            );

            return false;
        }

        if (mapData == null)
        {
            Debug.LogError(
                "MapData is missing."
            );

            return false;
        }

        if (terrainDatabase == null)
        {
            Debug.LogError(
                "TerrainDatabase is missing."
            );

            return false;
        }

        if (terrainDatabase.Terrains == null ||
            terrainDatabase.Terrains.Count == 0)
        {
            Debug.LogError(
                "TerrainDatabase contains no terrains."
            );

            return false;
        }

        return true;
    }

    private void GenerateHeightMap()
    {
        foreach (HexTile tile in allTiles)
        {
            float height =
                GetNoise(
                    tile,
                    mapData.heightScale
                );

            tile.SetHeight(height);
        }
    }

    private void GenerateMoistureMap()
    {
        foreach (HexTile tile in allTiles)
        {
            float moisture =
                GetNoise(
                    tile,
                    mapData.moistureScale
                );

            tile.SetMoisture(moisture);
        }
    }

    private float GetNoise(
        HexTile tile,
        float scale)
    {
        float x =
            tile.Coordinates.Q *
            scale;

        float y =
            tile.Coordinates.R *
            scale;

        float seedOffset =
            currentSeed * 0.001f;

        float value =
            Mathf.PerlinNoise(
                x + seedOffset,
                y + seedOffset
            );

        return Mathf.Clamp01(value);
    }

    private void GenerateTerrains()
    {
        foreach (HexTile tile in allTiles)
        {
            TerrainData terrain =
                FindTerrainForTile(tile);

            if (terrain != null)
                tile.SetTerrain(terrain);
        }
    }

    private TerrainData FindTerrainForTile(
        HexTile tile)
    {
        TerrainData selectedTerrain = null;

        foreach (TerrainData terrain
                 in terrainDatabase.Terrains)
        {
            if (terrain == null)
                continue;

            if (!terrain.canGenerate)
                continue;

            if (!MatchesHeight(
                    tile,
                    terrain))
            {
                continue;
            }

            if (!MatchesMoisture(
                    tile,
                    terrain))
            {
                continue;
            }

            if (!PassesSpawnChance(
                    terrain.spawnChance))
            {
                continue;
            }

            if (selectedTerrain == null ||
                terrain.priority >
                selectedTerrain.priority)
            {
                selectedTerrain = terrain;
            }
        }

        return selectedTerrain;
    }

    private bool MatchesHeight(
        HexTile tile,
        TerrainData terrain)
    {
        return
            tile.Height >= terrain.minHeight &&
            tile.Height <= terrain.maxHeight;
    }

    private bool MatchesMoisture(
        HexTile tile,
        TerrainData terrain)
    {
        return
            tile.Moisture >= terrain.minMoisture &&
            tile.Moisture <= terrain.maxMoisture;
    }

    private bool PassesSpawnChance(
        float spawnChance)
    {
        float value =
            (float)random.NextDouble();

        return value <= spawnChance;
    }

    private void FillEmptyTiles()
    {
        TerrainData fallbackTerrain =
            FindFallbackTerrain();

        if (fallbackTerrain == null)
        {
            Debug.LogError(
                "No fallback terrain found."
            );

            return;
        }

        foreach (HexTile tile in allTiles)
        {
            if (tile.HasTerrain)
                continue;

            tile.SetTerrain(
                fallbackTerrain
            );
        }
    }

    private TerrainData FindFallbackTerrain()
    {
        TerrainData fallback = null;

        foreach (TerrainData terrain
                 in terrainDatabase.Terrains)
        {
            if (terrain == null)
                continue;

            if (!terrain.isFallback)
                continue;

            if (fallback == null ||
                terrain.priority >
                fallback.priority)
            {
                fallback = terrain;
            }
        }

        return fallback;
    }

    public int GetTerrainId(TerrainData terrain)
    {
        if (terrain == null || terrainDatabase == null)
            return -1;

        for (int i = 0; i < terrainDatabase.Terrains.Count; i++)
        {
            if (terrainDatabase.Terrains[i] == terrain)
                return i;
        }

        return -1;
    }

    public bool PrepareNetworkGrid(int width, int height)
    {
        if (!ValidateReferences())
            return false;

        grid.ClearGrid();

        // Якщо сітка вже створена, не створюємо тайли повторно.
        if (grid.GetAllTiles().Count == 0)
            grid.Generate(width, height);

        allTiles.Clear();
        allTiles.AddRange(grid.GetAllTiles());

        return allTiles.Count == width * height;
    }

    public void ApplyNetworkTileData(
        int q,
        int r,
        int terrainId,
        float height,
        float moisture)
    {
        HexTile tile = grid.GetTile(new HexCoordinates(q, r));

        if (tile == null)
            return;

        if (terrainId < 0 ||
            terrainId >= terrainDatabase.Terrains.Count)
        {
            Debug.LogError(
                $"Invalid terrain ID {terrainId} at ({q}, {r}).");
            return;
        }

        tile.SetHeight(height);
        tile.SetMoisture(moisture);
        tile.SetTerrain(terrainDatabase.Terrains[terrainId]);
    }

    private void InitializeSeed()
    {
        if (mapData.useRandomSeed)
        {
            currentSeed =
                UnityEngine.Random.Range(
                    int.MinValue,
                    int.MaxValue
                );
        }
        else
        {
            currentSeed =
                mapData.seed;
        }

        random =
            new System.Random(
                currentSeed
            );
    }
}