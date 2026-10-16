using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityBuildPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform content;
    [SerializeField] private Button buildingButtonPrefab;

    [Header("Gameplay")]
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private BuildingPlacementSystem placementSystem;
    [SerializeField] private TurnManager turnManager;

    private SelectionSystem selectionSystem;
    private City selectedCity;

    private void Start()
    {
        selectionSystem =
            FindFirstObjectByType<SelectionSystem>();

        if (selectionSystem == null)
        {
            Debug.LogError(
                "SelectionSystem not found."
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

        selectionSystem.OnCitySelected += ShowCity;
        selectionSystem.OnUnitSelected += HideForUnit;
        selectionSystem.OnTileSelected += HideForTile;
        selectionSystem.OnSelectionCleared += Hide;

        Hide();
    }

    private void OnDestroy()
    {
        if (selectionSystem == null)
            return;

        selectionSystem.OnCitySelected -= ShowCity;
        selectionSystem.OnUnitSelected -= HideForUnit;
        selectionSystem.OnTileSelected -= HideForTile;
        selectionSystem.OnSelectionCleared -= Hide;
    }

    private void ShowCity(City city)
    {
        if (city == null)
        {
            Hide();
            return;
        }

        selectedCity = city;

        GenerateBuildingButtons();

        if (panel != null)
            panel.SetActive(true);
    }

    public void Open(City city)
    {
        if (city == null)
            return;

        selectedCity = city;

        if (panel != null)
            panel.SetActive(true);

        GenerateBuildingButtons();
    }

    private void GenerateBuildingButtons()
    {
        ClearButtons();

        if (selectedCity == null)
            return;

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

        // Не дозволяємо будувати
        // у чужому місті.
        if (selectedCity.Owner != currentPlayer)
        {
            return;
        }

        IReadOnlyList<BuildingData> buildings =
            buildingSystem.GetAvailableBuildings(
                selectedCity
            );

        foreach (BuildingData buildingData in buildings)
        {
            if (buildingData == null)
                continue;

            CreateBuildingButton(
                currentPlayer,
                buildingData
            );
        }
    }

    private void CreateBuildingButton(
        Player player,
        BuildingData buildingData)
    {
        if (buildingButtonPrefab == null)
        {
            Debug.LogError(
                "Building Button Prefab is missing."
            );

            return;
        }

        if (content == null)
        {
            Debug.LogError(
                "Content is missing."
            );

            return;
        }

        Button button =
            Instantiate(
                buildingButtonPrefab,
                content
            );

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text =
                $"{buildingData.buildingName} " +
                $"({buildingData.productionCost} $)";
        }

        BuildingData selectedBuilding =
            buildingData;

        bool canBuild =
            CanStartBuilding(
                player,
                selectedBuilding
            );

        button.interactable =
            canBuild;

        button.onClick.AddListener(
            () =>
            {
                Build(selectedBuilding);
            }
        );
    }

    private bool CanStartBuilding(
        Player player,
        BuildingData buildingData)
    {
        if (player == null)
            return false;

        if (selectedCity == null)
            return false;

        if (buildingData == null)
            return false;

        if (selectedCity.Owner != player)
            return false;

        if (!selectedCity.CanBuildThisTurn())
            return false;

        if (player.Resources == null)
            return false;

        if (!player.Resources.CanAfford(
                buildingData.productionCost,
                buildingData.goldCost))
        {
            return false;
        }

        if (buildingData.technologyRequirement != null)
        {
            if (!player.HasTechnology(
                    buildingData.technologyRequirement))
            {
                return false;
            }
        }

        return true;
    }

    private void Build(
        BuildingData buildingData)
    {
        if (selectedCity == null)
            return;

        if (buildingData == null)
            return;

        if (placementSystem == null)
        {
            Debug.LogError(
                "BuildingPlacementSystem is missing."
            );

            return;
        }

        if (turnManager == null)
            return;

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
            return;

        if (selectedCity.Owner != currentPlayer)
        {
            Debug.Log(
                "Cannot build in another player's city."
            );

            return;
        }

        placementSystem.StartPlacement(
            selectedCity,
            buildingData
        );
    }

    public void Close()
    {
        selectedCity = null;

        ClearButtons();

        if (panel != null)
            panel.SetActive(false);
    }

    private void ClearButtons()
    {
        if (content == null)
            return;

        for (int i = content.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                content.GetChild(i).gameObject
            );
        }
    }

    private void HideForUnit(Unit unit)
    {
        Hide();
    }

    private void HideForTile(HexTile tile)
    {
        Hide();
    }

    private void Hide()
    {
        selectedCity = null;

        if (panel != null)
            panel.SetActive(false);

        ClearButtons();
    }
}