using UnityEngine;
using TMPro;

public class TileInfoPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;

    [SerializeField] private TMP_Text terrainNameText;
    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private TMP_Text movementCostText;
    [SerializeField] private TMP_Text defenseBonusText;

    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text productionText;
    [SerializeField] private TMP_Text goldText;

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

        selectionSystem.OnTileSelected += ShowTile;
        selectionSystem.OnUnitSelected += HandleUnitSelected;
        selectionSystem.OnSelectionCleared += Hide;

        Hide();
    }

    private void OnDestroy()
    {
        if (selectionSystem == null)
            return;

        selectionSystem.OnTileSelected -= ShowTile;
        selectionSystem.OnUnitSelected -= HandleUnitSelected;
        selectionSystem.OnSelectionCleared -= Hide;
    }

    private void ShowTile(HexTile tile)
    {
        if (tile == null)
        {
            Hide();
            return;
        }

        if (!tile.HasTerrain)
        {
            Hide();
            return;
        }

        TerrainData terrain =
            tile.Terrain;

        terrainNameText.text =
            terrain.terrainName;

        descriptionText.text =
            terrain.description;

        movementCostText.text =
            $"Movement: {terrain.movementCost}";

        defenseBonusText.text =
            $"Defense: +{terrain.defenseBonus}";

        foodText.text =
            $"Food: {terrain.food}";

        productionText.text =
            $"Production: {terrain.production}";

        goldText.text =
            $"Gold: {terrain.gold}";

        panel.SetActive(true);
    }

    private void HandleUnitSelected(Unit unit)
    {
        Hide();
    }

    private void Hide()
    {
        panel.SetActive(false);
    }
}