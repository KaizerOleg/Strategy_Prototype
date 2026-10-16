using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text tierText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private Button learnButton;

    [Header("Highlight")]
    [SerializeField] private Image highlightImage;

    private TechnologyData technology;
    private TechnologySystem technologySystem;
    private TechnologyUI technologyUI;
    private TurnManager turnManager;

    public TechnologyData Technology => technology;

    public void Initialize(
        TechnologyData technology,
        TechnologySystem technologySystem,
        TechnologyUI technologyUI,
        TurnManager turnManager)
    {
        this.technology = technology;
        this.technologySystem = technologySystem;
        this.technologyUI = technologyUI;
        this.turnManager = turnManager;

        if (technology == null)
        {
            Debug.LogError(
                "TechnologyData is missing."
            );

            return;
        }

        if (technologySystem == null)
        {
            Debug.LogError(
                "TechnologySystem is missing."
            );

            return;
        }

        if (technologyUI == null)
        {
            Debug.LogError(
                "TechnologyUI is missing."
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

        UpdateUI();

        if (learnButton != null)
        {
            learnButton.onClick.RemoveAllListeners();

            learnButton.onClick.AddListener(
                OnLearnClicked
            );
        }
    }

    private void UpdateUI()
    {
        if (technology == null)
            return;

        if (technologySystem == null)
            return;

        if (turnManager == null)
            return;

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
            return;

        if (nameText != null)
        {
            nameText.text =
                technology.technologyName;
        }

        if (tierText != null)
        {
            tierText.text =
                $"Tier {technology.tier}";
        }

        if (costText != null)
        {
            costText.text =
                $"Science: {technology.researchCost}";
        }

        bool researched =
            technologySystem.HasTechnology(
                currentPlayer,
                technology
            );

        bool canResearch =
            technologySystem.CanResearch(
                currentPlayer,
                technology
            );

        if (researched)
        {
            if (statusText != null)
                statusText.text = "Researched";

            if (learnButton != null)
                learnButton.interactable = false;
        }
        else if (canResearch)
        {
            if (statusText != null)
                statusText.text = "Available";

            if (learnButton != null)
                learnButton.interactable = true;
        }
        else
        {
            if (statusText != null)
                statusText.text = "Locked";

            // Залишаємо кнопку активною,
            // щоб можна було побачити
            // відсутні вимоги.
            if (learnButton != null)
                learnButton.interactable = true;
        }

        SetHighlighted(false);
    }

    private void OnLearnClicked()
    {
        if (technology == null)
            return;

        if (technologySystem == null)
            return;

        if (technologyUI == null)
            return;

        if (turnManager == null)
            return;

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
            return;

        if (technologySystem.HasTechnology(
                currentPlayer,
                technology))
        {
            return;
        }

        if (technologySystem.CanResearch(
                currentPlayer,
                technology))
        {
            technologySystem.Research(
                currentPlayer,
                technology
            );

            return;
        }

        HighlightMissingRequirements(
            currentPlayer
        );
    }

    private void HighlightMissingRequirements(
        Player player)
    {
        if (player == null)
            return;

        if (technology == null)
            return;

        if (technology.requiredTechnologies == null ||
            technology.requiredTechnologies.Length == 0)
        {
            return;
        }

        bool hasMissingRequirements = false;

        foreach (
            TechnologyData requirement
            in technology.requiredTechnologies)
        {
            if (requirement == null)
                continue;

            if (!technologySystem.HasTechnology(
                    player,
                    requirement))
            {
                hasMissingRequirements = true;
                break;
            }
        }

        if (!hasMissingRequirements)
            return;

        technologyUI.HighlightRequirements(
            technology.requiredTechnologies
        );
    }

    public void SetHighlightAlpha(float alpha)
    {
        if (highlightImage == null)
            return;

        Color color =
            highlightImage.color;

        color.a = alpha;

        highlightImage.color = color;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightImage != null)
            highlightImage.enabled = highlighted;
    }
}