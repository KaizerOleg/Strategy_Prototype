using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class BuildingPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private HexGrid grid;
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private SelectionSystem selectionSystem;
    [SerializeField] private TurnManager turnManager;

    private readonly List<HexTile> highlightedTiles = new();

    private City selectedCity;
    private BuildingData selectedBuilding;

    public bool IsPlacing { get; private set; }

    public void StartPlacement(
        City city,
        BuildingData buildingData)
    {
        if (city == null)
        {
            Debug.LogError(
                "City is missing."
            );

            return;
        }

        if (buildingData == null)
        {
            Debug.LogError(
                "BuildingData is missing."
            );

            return;
        }

        if (buildingSystem == null)
        {
            Debug.LogError(
                "BuildingSystem is missing."
            );

            return;
        }

        if (turnManager == null)
        {
            Debug.LogError(
                "TurnManager is missing."
            );

            return;
        }

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
        {
            Debug.LogError(
                "CurrentPlayer is missing."
            );

            return;
        }

        // Не дозволяємо будувати в чужому місті.
        if (city.Owner != currentPlayer)
        {
            Debug.Log(
                "Cannot build in another player's city."
            );

            return;
        }

        selectedCity = city;
        selectedBuilding = buildingData;

        IsPlacing = true;

        if (selectionSystem != null)
        {
            selectionSystem.SetInputEnabled(false);
        }

        ShowBuildableTiles();
    }

    private void Update()
    {
        if (!IsPlacing)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StopPlacement();
            return;
        }

        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (IsPointerOverUI())
            return;

        HandleTileClick();
    }

    private void HandleTileClick()
    {
        if (mainCamera == null)
            return;

        if (turnManager == null)
            return;

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
            return;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        Collider2D[] hits =
            Physics2D.OverlapPointAll(
                mousePosition
            );

        HexTile tile =
            FindTile(hits);

        if (tile == null)
            return;

        if (!IsTileBuildable(tile))
        {
            Debug.Log(
                "Cannot build on this tile."
            );

            return;
        }

        NetworkManager networkManager = NetworkManager.Singleton;

        bool isNetworkGame =
            networkManager != null && networkManager.IsListening;

        if (isNetworkGame)
        {
            NetworkCity networkCity =
                selectedCity.GetComponent<NetworkCity>();

            int buildingTypeId =
                buildingSystem.GetBuildingTypeId(
                    currentPlayer,
                    selectedBuilding);

            if (networkCity == null ||
                !networkCity.IsSpawned ||
                buildingTypeId < 0)
            {
                Debug.LogError(
                    "Cannot request building: NetworkCity or BuildingData ID is missing.");
                return;
            }

            // Клієнт надсилає запит місту.
            // Сервер виконає перевірки та створить будівлю.
            bool requestSent = networkCity.RequestBuild(
                tile.Coordinates.Q,
                tile.Coordinates.R,
                buildingTypeId);

            if (requestSent)
                StopPlacement();

            return;
        }

        // Локальна гра — без мережевого RPC.
        bool success = buildingSystem.Build(
            currentPlayer,
            selectedCity,
            tile,
            selectedBuilding);

        if (success)
            StopPlacement();
    }

    private void ShowBuildableTiles()
    {
        ClearPlacementVisuals();

        if (grid == null)
            return;

        foreach (HexTile tile in grid.GetAllTiles())
        {
            if (tile == null)
                continue;

            bool canBuild =
                IsTileBuildable(tile);

            if (tile.View != null)
            {
                tile.View.SetBuildPlacementBlocked(
                    !canBuild
                );
            }

            highlightedTiles.Add(tile);
        }
    }

    private bool IsTileBuildable(
        HexTile tile)
    {
        if (tile == null)
            return false;

        if (selectedCity == null)
            return false;

        if (selectedBuilding == null)
            return false;

        if (buildingSystem == null)
            return false;

        if (turnManager == null)
            return false;

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
            return false;

        if (selectedCity.Owner != currentPlayer)
            return false;

        return buildingSystem.CanBuild(
            currentPlayer,
            selectedCity,
            tile,
            selectedBuilding
        );
    }

    private HexTile FindTile(
        Collider2D[] hits)
    {
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            HexTile tile =
                hit.GetComponent<HexTile>();

            if (tile != null)
                return tile;
        }

        return null;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    public void StopPlacement()
    {
        IsPlacing = false;

        ClearPlacementVisuals();

        selectedCity = null;
        selectedBuilding = null;

        if (selectionSystem != null)
        {
            selectionSystem.SetInputEnabled(true);
        }
    }

    private void ClearPlacementVisuals()
    {
        foreach (HexTile tile in highlightedTiles)
        {
            if (tile == null ||
                tile.View == null)
            {
                continue;
            }

            tile.View.SetBuildPlacementBlocked(
                false
            );
        }

        highlightedTiles.Clear();
    }
}