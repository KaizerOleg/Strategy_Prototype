using UnityEngine;
using TMPro;

public class UnitInfoUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;

    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text movementText;

    private SelectionSystem selectionSystem;

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

        selectionSystem.OnUnitSelected += ShowUnit;
        selectionSystem.OnTileSelected += HideForTile;
        selectionSystem.OnSelectionCleared += Hide;

        Hide();
    }

    private void OnDestroy()
    {
        if (selectionSystem == null)
            return;

        selectionSystem.OnUnitSelected -= ShowUnit;
        selectionSystem.OnTileSelected -= HideForTile;
        selectionSystem.OnSelectionCleared -= Hide;
    }

    private void ShowUnit(Unit unit)
    {
        if (unit == null ||
            unit.Data == null)
        {
            Hide();
            return;
        }

        unitNameText.text =
            unit.Data.unitName;

        healthText.text =
            $"HP: {unit.CurrentHealth} / {unit.Data.maxHealth}";

        attackText.text =
            $"Attack: {unit.GetAttack()}";

        defenseText.text =
            $"Defense: {unit.Data.defense}";

        movementText.text =
            $"Movement: {unit.Data.movementRange}";

        panel.SetActive(true);
    }

    private void Hide()
    {
        panel.SetActive(false);
    }

    private void HideForTile(HexTile tile)
    {
        Hide();
    }
}