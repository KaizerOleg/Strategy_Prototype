
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectionSystem : MonoBehaviour
{
    public Unit SelectedUnit { get; private set; }
    public HexTile SelectedTile { get; private set; }

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private FeatureSystem featureSystem;
    [SerializeField] private FeatureTargetVisualizer featureTargetVisualizer;
    [SerializeField] private EffectSystem effectSystem;

    public event Action<Unit> OnUnitSelected;
    public event Action<HexTile> OnTileSelected;
    public event Action<City> OnCitySelected;
    public event Action OnSelectionCleared;

    private NetworkUnit subscribedNetworkUnit;

    private bool inputEnabled = true;

    private Unit featureSource;
    private Feature selectedFeature;
    private bool isFeatureTargeting;

    private void Start()
    {
        if (turnManager == null)
        {
            Debug.LogError(
                "[SelectionSystem] TurnManager is missing.");
            return;
        }

        if (playerManager == null)
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }

        if (playerManager == null)
        {
            Debug.LogError(
                "[SelectionSystem] PlayerManager is missing.");
            return;
        }

        turnManager.OnPlayerTurnStarted += OnPlayerTurnStarted;
    }

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnPlayerTurnStarted -= OnPlayerTurnStarted;
        }

        UnsubscribeFromSelectedUnit();
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (IsPointerOverUI())
            return;

        HandleClick();
    }

    private void OnPlayerTurnStarted(Player player)
    {
        ClearSelection();
        inputEnabled = true;
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (!enabled)
            ClearSelection();
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position =
            Mouse.current.position.ReadValue();

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
    }


    private bool IsLocalPlayersTurn()
    {
        if (playerManager == null || turnManager == null)
            return false;

        Player localPlayer = playerManager.LocalPlayer;

        if (localPlayer == null)
            return false;

        if (!NetworkManagerHelper.IsNetworked)
            return turnManager.CurrentPlayer == localPlayer;

        return turnManager.CurrentPlayer == localPlayer;
    }

    private void HandleClick()
    {
        if (mainCamera == null || turnManager == null)
            return;

        if (playerManager == null)
            return;

        Player localPlayer = playerManager.LocalPlayer;

        if (localPlayer == null)
        {
            Debug.LogWarning(
                "[SelectionSystem] LocalPlayer is null.");
            return;
        }

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(
                Mouse.current.position.ReadValue());

        Collider2D[] hits =
            Physics2D.OverlapPointAll(mousePosition);

        if (hits.Length == 0)
        {
            ClearSelection();
            return;
        }

        Unit unit = FindUnit(hits);

        if (isFeatureTargeting)
        {
            HandleFeatureTarget(unit);
            return;
        }

        if (unit != null)
        {
            SelectUnit(unit);
            return;
        }

        HexTile tile = FindTile(hits);

        if (tile != null)
        {
            HandleTileClick(tile);
            return;
        }

        ClearSelection();
    }

    #region Feature

    public void StartFeatureTargeting(Unit source, Feature feature)
    {
        if (source == null || feature == null || !feature.RequiresTarget)
            return;

        if (playerManager == null ||
            source.Owner != playerManager.LocalPlayer ||
            !IsLocalPlayersTurn())
        {
            return;
        }

        CancelFeatureTargeting();

        featureSource = source;
        selectedFeature = feature;
        isFeatureTargeting = true;

        ShowFeatureTargetRange();
    }

    private void ShowFeatureTargetRange()
    {
        if (featureTargetVisualizer == null || featureSource == null)
            return;

        if (selectedFeature is AttackFeature attackFeature)
        {
            featureTargetVisualizer.ShowAttackRange(
                featureSource,
                attackFeature.attackRange);
        }
        else if (selectedFeature is FireAttackFeature fireAttackFeature)
        {
            featureTargetVisualizer.ShowAttackRange(
                featureSource,
                fireAttackFeature.attackRange);
        }
    }

    private void HandleFeatureTarget(Unit targetUnit)
    {
        if (!isFeatureTargeting)
            return;

        if (featureSource == null || selectedFeature == null)
        {
            CancelFeatureTargeting();
            return;
        }

        if (!IsLocalPlayersTurn())
            return;

        if (targetUnit == null)
            return;

        if (featureSource.Owner != playerManager.LocalPlayer)
            return;

        NetworkUnit networkUnit =
            featureSource.GetComponent<NetworkUnit>();

        if (networkUnit != null && NetworkManagerHelper.IsNetworked)
        {
            bool sent = networkUnit.RequestActivateFeature(
                selectedFeature,
                targetUnit);

            if (sent)
                CancelFeatureTargeting();

            return;
        }

        FeatureContext context = new FeatureContext(
            unit: featureSource,
            player: featureSource.Owner,
            target: new FeatureTarget(unit: targetUnit),
            effectSystem: effectSystem);

        if (featureSystem != null &&
            featureSystem.Activate(selectedFeature, context))
        {
            CancelFeatureTargeting();
        }
    }

    public void CancelFeatureTargeting()
    {
        if (featureTargetVisualizer != null)
            featureTargetVisualizer.Clear();

        featureSource = null;
        selectedFeature = null;
        isFeatureTargeting = false;
    }

    #endregion
    #region Selection

    private Unit FindUnit(Collider2D[] hits)
    {
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Unit unit = hit.GetComponent<Unit>();

            if (unit != null)
                return unit;
        }

        return null;
    }

    private HexTile FindTile(Collider2D[] hits)
    {
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            HexTile tile = hit.GetComponent<HexTile>();

            if (tile != null)
                return tile;
        }

        return null;
    }

    private void SelectUnit(Unit unit)
    {
        if (unit == null || playerManager == null)
            return;

        Player localPlayer = playerManager.LocalPlayer;

        if (localPlayer == null || unit.Owner != localPlayer)
            return;

        if (!IsLocalPlayersTurn())
            return;

        ClearSelection();

        SelectedUnit = unit;
        SelectedTile = unit.CurrentTile;

        SubscribeToSelectedUnit(unit);

        SelectedUnit.HideMovementRange();
        SelectedUnit.ShowMovementRange();

        OnUnitSelected?.Invoke(unit);
    }

    private void SelectTile(HexTile tile)
    {
        if (tile == null)
            return;

        ClearSelection();

        SelectedTile = tile;

        OnTileSelected?.Invoke(tile);
    }

    private void SelectCity(City city)
    {
        if (city == null || playerManager == null)
            return;

        Player localPlayer = playerManager.LocalPlayer;

        if (localPlayer == null)
        {
            Debug.LogWarning(
                "[SelectionSystem] LocalPlayer is null.");
            return;
        }

        if (city.Owner == null)
        {
            Debug.LogWarning(
                "[SelectionSystem] City has no Owner.");
            return;
        }

        if (city.Owner != localPlayer)
        {
            ClearSelection();
            return;
        }

        ClearSelection();

        SelectedTile = city.CenterTile;

        OnCitySelected?.Invoke(city);
    }

    private void HandleTileClick(HexTile tile)
    {
        if (tile == null)
            return;

        if (SelectedUnit != null)
        {
            HandleSelectedUnit(tile);
            return;
        }

        if (tile.HasCity() && tile.City != null)
        {
            SelectCity(tile.City);
            return;
        }

        SelectTile(tile);
    }

    private void HandleSelectedUnit(HexTile tile)
    {
        if (SelectedUnit == null || tile == null)
            return;

        if (playerManager == null)
            return;

        Player localPlayer = playerManager.LocalPlayer;

        if (localPlayer == null ||
            SelectedUnit.Owner != localPlayer ||
            !IsLocalPlayersTurn())
        {
            ClearSelection();
            return;
        }

        UnitMovement movement =
            SelectedUnit.GetComponent<UnitMovement>();

        if (movement == null)
        {
            Debug.LogError(
                "[SelectionSystem] UnitMovement is missing.");
            return;
        }

        NetworkUnit networkUnit =
            SelectedUnit.GetComponent<NetworkUnit>();

        if (networkUnit != null && NetworkManagerHelper.IsNetworked)
        {
            networkUnit.RequestMove(
                tile.Coordinates.Q,
                tile.Coordinates.R);

            return;
        }

        if (playerManager == null ||
        playerManager.LocalPlayer == null ||
        SelectedUnit.Owner != playerManager.LocalPlayer ||
        turnManager.CurrentPlayer != playerManager.LocalPlayer)
        {
            return;
        }

        movement.TryMoveTo(tile);
    }

    public void ClearSelection()
    {
        if (SelectedUnit != null)
            SelectedUnit.HideMovementRange();

        UnsubscribeFromSelectedUnit();
        CancelFeatureTargeting();

        SelectedUnit = null;
        SelectedTile = null;

        OnSelectionCleared?.Invoke();
    }

    #endregion
    #region NetSelect
    private void SubscribeToSelectedUnit(Unit unit)
    {
        UnsubscribeFromSelectedUnit();

        if (unit == null)
            return;

        subscribedNetworkUnit =
            unit.GetComponent<NetworkUnit>();

        if (subscribedNetworkUnit != null)
        {
            subscribedNetworkUnit.OnNetworkTileUpdated +=
                OnSelectedUnitNetworkTileUpdated;
        }
    }

    private void UnsubscribeFromSelectedUnit()
    {
        if (subscribedNetworkUnit != null)
        {
            subscribedNetworkUnit.OnNetworkTileUpdated -=
                OnSelectedUnitNetworkTileUpdated;

            subscribedNetworkUnit = null;
        }
    }

    private void OnSelectedUnitNetworkTileUpdated(Unit unit)
    {
        if (unit == null || unit != SelectedUnit)
            return;

        SelectedTile = unit.CurrentTile;

        unit.HideMovementRange();
        unit.ShowMovementRange();
    }

    #endregion
}
