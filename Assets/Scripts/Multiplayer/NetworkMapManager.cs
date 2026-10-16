using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkMapManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private HexGrid grid;
    [SerializeField] private MapData mapData;

    public NetworkVariable<int> MapWidth =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> MapHeight =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private NetworkList<NetworkMapTile> networkTiles;

    private Coroutine applyMapCoroutine;
    private bool mapApplied;

    private void Awake()
    {
        networkTiles = new NetworkList<NetworkMapTile>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        MapWidth.OnValueChanged += OnMapSizeChanged;
        MapHeight.OnValueChanged += OnMapSizeChanged;
        networkTiles.OnListChanged += OnMapTilesChanged;

        mapApplied = false;

        if (IsServer)
        {
            GenerateAndPublishMap();
        }
        else
        {
            StartApplyingMap();
        }
    }

    public override void OnNetworkDespawn()
    {
        MapWidth.OnValueChanged -= OnMapSizeChanged;
        MapHeight.OnValueChanged -= OnMapSizeChanged;
        networkTiles.OnListChanged -= OnMapTilesChanged;

        mapApplied = false;

        if (applyMapCoroutine != null)
        {
            StopCoroutine(applyMapCoroutine);
            applyMapCoroutine = null;
        }

        if (applyMapCoroutine != null)
        {
            StopCoroutine(applyMapCoroutine);
            applyMapCoroutine = null;
        }

        base.OnNetworkDespawn();
    }

    private void GenerateAndPublishMap()
    {
        if (!ValidateReferences())
            return;

        networkTiles.Clear();

        MapWidth.Value = mapData.width;
        MapHeight.Value = mapData.height;

        mapGenerator.GenerateMap();

        var tiles = grid.GetAllTiles();

        if (tiles.Count != mapData.width * mapData.height)
        {
            Debug.LogError(
                "[NetworkMapManager] Map generation failed or grid size is incorrect.");
            return;
        }

        foreach (HexTile tile in tiles)
        {
            int terrainId =
                mapGenerator.GetTerrainId(tile.Terrain);

            if (terrainId < 0)
            {
                Debug.LogError(
                    $"[NetworkMapManager] Terrain ID not found " +
                    $"at ({tile.Coordinates.Q}, {tile.Coordinates.R}).");
                return;
            }

            networkTiles.Add(
                new NetworkMapTile(
                    tile.Coordinates.Q,
                    tile.Coordinates.R,
                    terrainId,
                    tile.Height,
                    tile.Moisture));
        }

        Debug.Log(
            $"[NetworkMapManager] Published map: " +
            $"{MapWidth.Value}x{MapHeight.Value}, " +
            $"tiles={networkTiles.Count}.");
    }

    private bool ValidateReferences()
    {
        if (mapGenerator == null ||
            grid == null ||
            mapData == null)
        {
            Debug.LogError(
                "[NetworkMapManager] MapGenerator, HexGrid or MapData is missing.",
                this);
            return false;
        }

        return true;
    }

    private void OnMapSizeChanged(int previous, int current)
    {
        if (!IsServer)
            StartApplyingMap();
    }

    private void OnMapTilesChanged(
        NetworkListEvent<NetworkMapTile> changeEvent)
    {
        if (!IsServer)
            StartApplyingMap();
    }

    private void StartApplyingMap()
    {
        if (IsServer || !IsSpawned || mapApplied)
            return;

        if (applyMapCoroutine == null)
            applyMapCoroutine = StartCoroutine(ApplyMapRoutine());
    }

    private IEnumerator ApplyMapRoutine()
    {
        while (IsSpawned && !mapApplied)
        {
            if (MapWidth.Value > 0 &&
                MapHeight.Value > 0 &&
                networkTiles.Count == MapWidth.Value * MapHeight.Value)
            {
                if (ApplyReceivedMap())
                {
                    mapApplied = true;
                    applyMapCoroutine = null;

                    Debug.Log(
                        $"[NetworkMapManager] Client map applied: " +
                        $"{MapWidth.Value}x{MapHeight.Value}.");

                    yield break;
                }
            }

            yield return null;
        }

        applyMapCoroutine = null;
    }

    private bool ApplyReceivedMap()
    {
        if (!ValidateReferences())
            return false;

        if (!mapGenerator.PrepareNetworkGrid(
                MapWidth.Value,
                MapHeight.Value))
        {
            return false;
        }

        grid.ClearTileOwners();

        foreach (NetworkMapTile tile in networkTiles)
        {
            mapGenerator.ApplyNetworkTileData(
                tile.Q,
                tile.R,
                tile.TerrainId,
                tile.Height,
                tile.Moisture);
        }

        return true;
    }
}