using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeatureButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text featureNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private Button button;

    private Feature feature;
    private FeatureContext context;
    private FeatureSystem featureSystem;
    private SelectionSystem selectionSystem;

    public void Initialize(
        Feature feature,
        FeatureContext context,
        FeatureSystem featureSystem,
        SelectionSystem selectionSystem)
    {
        this.feature = feature;
        this.context = context;
        this.featureSystem = featureSystem;
        this.selectionSystem = selectionSystem;

        if (featureNameText != null)
            featureNameText.text = feature.FeatureName;

        if (descriptionText != null)
            descriptionText.text = feature.Description;

        if (typeText != null)
            typeText.text = feature.IsPassive
                ? "Passive"
                : "Active";

        ConfigureButton();
    }

    private void ConfigureButton()
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();

        // Passive Feature не можна активувати.
        if (feature.IsPassive)
        {
            button.interactable = false;
            return;
        }

        bool canActivate =
            featureSystem != null &&
            featureSystem.CanActivate(
                feature,
                context
            );

        button.interactable = canActivate;

        if (canActivate)
        {
            button.onClick.AddListener(
                ActivateFeature
            );
        }
    }

    private void ActivateFeature()
    {
        if (feature == null || featureSystem == null)
            return;

        if (context == null || context.Unit == null)
        {
            Debug.LogWarning(
                "Мережева активація здібностей міст " +
                "поки не підтримується."
            );

            return;
        }

        if (feature.RequiresTarget)
        {
            if (selectionSystem == null)
            {
                Debug.LogError("SelectionSystem is missing.");
                return;
            }

            context.Unit.HideMovementRange();

            selectionSystem.StartFeatureTargeting(
                context.Unit,
                feature
            );

            return;
        }

        NetworkUnit networkUnit =
            context.Unit.GetComponent<NetworkUnit>();

        if (networkUnit != null &&
            NetworkManagerHelper.IsNetworked)
        {
            if (networkUnit.RequestActivateFeature(feature))
            {
                selectionSystem?.ClearSelection();
            }

            return;
        }

        // Звичайний локальний режим.
        bool activated =
            featureSystem.Activate(feature, context);

        if (activated)
            ConfigureButton();
    }
}