using UnityEngine;
using UnityEngine.UI;

public class UnitActionPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button foundCityButton;

    [Header("Gameplay")]
    [SerializeField] private CityManager cityManager;
    [SerializeField] private TurnManager turnManager;

    private SelectionSystem selectionSystem;

    private Unit selectedUnit;

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

        if (foundCityButton == null)
        {
            Debug.LogError(
                "Found City Button is missing."
            );

            return;
        }

        selectionSystem.OnUnitSelected += ShowUnitActions;
        selectionSystem.OnTileSelected += HideForTile;
        selectionSystem.OnCitySelected += HideForCity;
        selectionSystem.OnSelectionCleared += Hide;

        /*foundCityButton.onClick.AddListener(
            FoundCity
        );*/

        Hide();
    }

    private void OnDestroy()
    {
        if (selectionSystem != null)
        {
            selectionSystem.OnUnitSelected -= ShowUnitActions;
            selectionSystem.OnTileSelected -= HideForTile;
            selectionSystem.OnCitySelected -= HideForCity;
            selectionSystem.OnSelectionCleared -= Hide;
        }

        /*if (foundCityButton != null)
        {
            foundCityButton.onClick.RemoveListener(
                FoundCity
            );
        }*/
    }

    private void ShowUnitActions(Unit unit)
    {
        if (unit == null ||
            unit.Data == null)
        {
            Hide();
            return;
        }

        if (turnManager == null)
        {
            Hide();
            return;
        }

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
        {
            Hide();
            return;
        }

        if (unit.Owner != currentPlayer)
        {
            Hide();
            return;
        }

        selectedUnit = unit;

        bool canFoundCity =
            unit.Data.canFoundCity;

        foundCityButton.gameObject.SetActive(
            canFoundCity
        );

        if (panel != null)
        {
            panel.SetActive(
                canFoundCity
            );
        }
    }

    /*public void FoundCity()
    {
        if (selectedUnit == null)
            return;

        if (cityManager == null)
        {
            Debug.LogError(
                "CityManager is missing."
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

        if (selectedUnit.Owner != currentPlayer)
        {
            Debug.Log(
                "This unit does not belong to the current player."
            );

            return;
        }

        bool success =
            cityManager.FoundCity(
                selectedUnit,
                currentPlayer
            );

        if (!success)
            return;

        Hide();

        selectedUnit = null;

        if (selectionSystem != null)
        {
            selectionSystem.ClearSelection();
        }
    }*/

    private void HideForTile(HexTile tile)
    {
        Hide();
    }

    private void HideForCity(City city)
    {
        Hide();
    }

    private void Hide()
    {
        selectedUnit = null;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}