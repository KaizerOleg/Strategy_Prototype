using UnityEngine;
using TMPro;

public class CityInfoPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("City Info")]
    [SerializeField] private TMP_Text cityNameText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text productionText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text scienceText;

    [Header("Production")]
    [SerializeField] private TMP_Text buildingsText;
    [SerializeField] private TMP_Text unitProductionText;

    [Header("Systems")]
    [SerializeField] private EconomySystem economySystem;
    [SerializeField] private CityBuildPanel cityBuildPanel;
    [SerializeField] private UnitProductionPanel unitProductionPanel;
    [SerializeField] private TechnologySystem technologySystem;

    private TurnManager turnManager;
    private SelectionSystem selectionSystem;

    private City selectedCity;

    private void Start()
    {
        selectionSystem =
            FindFirstObjectByType<SelectionSystem>();

        turnManager =
            FindFirstObjectByType<TurnManager>();

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
                "TurnManager not found."
            );

            return;
        }

        selectionSystem.OnCitySelected += ShowCity;
        selectionSystem.OnSelectionCleared += Hide;
        selectionSystem.OnUnitSelected += HideForUnit;
        selectionSystem.OnTileSelected += HideForTile;

        turnManager.OnTurnStarted += OnTurnStarted;
        technologySystem.OnTechnologyResearched +=
            OnTechnologyResearched;

        Hide();
    }

    private void OnTechnologyResearched(
        Player player,
        TechnologyData technology)
    {
        if (turnManager == null)
            return;

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
            return;

        // ќновлюЇмо UI т≥льки дл€ поточного гравц€.
        if (player != currentPlayer)
            return;

        UpdateCityInfo();

        OpenProductionPanels();
    }

    private void OnTurnStarted(int turn)
    {
        if (selectedCity == null)
            return;

        UpdateCityInfo();

        OpenProductionPanels();
    }

    private void ShowCity(City city)
    {
        if (city == null ||
            city.Data == null)
        {
            Hide();
            return;
        }

        if (selectedCity != null)
        {
            selectedCity.OnProductionChanged -=
                OnCityProductionChanged;
        }

        selectedCity = city;

        selectedCity.OnProductionChanged +=
            OnCityProductionChanged;

        UpdateCityInfo();

        if (panel != null)
            panel.SetActive(true);

        OpenProductionPanels();
    }

    private void OnCityProductionChanged(int amount)
    {
        UpdateCityInfo();
    }

    private void UpdateCityInfo()
    {
        if (selectedCity == null)
            return;

        if (selectedCity.Data == null)
            return;

        cityNameText.text =
            selectedCity.Data.cityName;

        if (economySystem != null)
        {
            Yield income =
                economySystem.CalculateCityYield(
                    selectedCity
                );

            foodText.text =
                $"Food: +{income.food}";

            productionText.text =
                $"Production: +{income.production}";

            goldText.text =
                $"Gold: +{income.gold}";

            scienceText.text =
                $"Science: +{income.science}";
        }

        buildingsText.text =
            $"Buildings: " +
            $"{selectedCity.BuildingsBuiltThisTurn} / " +
            $"{selectedCity.Data.maxBuildingsPerTurn}";

        unitProductionText.text =
            $"Unit Production: " +
            $"{selectedCity.CurrentProduction}";
    }

    private void OpenProductionPanels()
    {
        if (selectedCity == null)
            return;

        if (cityBuildPanel != null)
            cityBuildPanel.Open(selectedCity);

        if (unitProductionPanel != null)
            unitProductionPanel.Open(selectedCity);
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
        if (selectedCity != null)
        {
            selectedCity.OnProductionChanged -=
                OnCityProductionChanged;
        }

        selectedCity = null;

        if (panel != null)
            panel.SetActive(false);

        if (cityBuildPanel != null)
            cityBuildPanel.Close();

        if (unitProductionPanel != null)
            unitProductionPanel.Close();
    }

    private void OnDestroy()
    {
        if (selectionSystem != null)
        {
            selectionSystem.OnCitySelected -= ShowCity;
            selectionSystem.OnSelectionCleared -= Hide;
            selectionSystem.OnUnitSelected -= HideForUnit;
            selectionSystem.OnTileSelected -= HideForTile;
        }

        if (turnManager != null)
        {
            turnManager.OnTurnStarted -= OnTurnStarted;
        }
        if(technologySystem != null)
        {
            technologySystem.OnTechnologyResearched -=
            OnTechnologyResearched;
        }

    }
}