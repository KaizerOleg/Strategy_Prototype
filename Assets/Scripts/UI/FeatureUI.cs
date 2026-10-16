using UnityEngine;

public class FeatureUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SelectionSystem selectionSystem;
    [SerializeField] private FeatureSystem featureSystem;
    [SerializeField] private FeatureButton featureButtonPrefab;
    [SerializeField] private Transform featureContainer;

    private void Start()
    {
        if (selectionSystem == null)
        {
            Debug.LogError(
                "SelectionSystem is missing in FeatureUI."
            );

            return;
        }

        if (featureSystem == null)
        {
            Debug.LogError(
                "FeatureSystem is missing in FeatureUI."
            );

            return;
        }

        selectionSystem.OnUnitSelected += OnUnitSelected;
        selectionSystem.OnCitySelected += OnCitySelected;
        selectionSystem.OnTileSelected += OnTileSelected;
        selectionSystem.OnSelectionCleared += Clear;

        featureSystem.OnFeatureActivated += OnFeatureActivated;
    }

    private void OnDestroy()
    {
        if (selectionSystem != null)
        {
            selectionSystem.OnUnitSelected -= OnUnitSelected;
            selectionSystem.OnCitySelected -= OnCitySelected;
            selectionSystem.OnTileSelected -= OnTileSelected;
            selectionSystem.OnSelectionCleared -= Clear;
        }

        if (featureSystem != null)
        {

            featureSystem.OnFeatureActivated -=
                OnFeatureActivated;
        }
    }

    private void OnFeatureActivated()
    {
        Clear();

        if (selectionSystem != null)
        {
            selectionSystem.ClearSelection();
        }
    }

    private void OnUnitSelected(Unit unit)
    {
        if (unit == null || unit.Data == null)
        {
            Clear();
            return;
        }

        ShowFeatures(unit.Data.Features, unit);
    }

    private void OnCitySelected(City city)
    {
        if (city == null || city.Data == null)
        {
            Clear();
            return;
        }

        ShowFeatures(city.Data.Features, null, city);
    }

    private void OnTileSelected(HexTile tile)
    {
        Clear();
    }

    private void ShowFeatures(
        System.Collections.Generic.IReadOnlyList<Feature> features,
        Unit unit = null,
        City city = null)
    {
        Clear();

        if (features == null || features.Count == 0)
            return;

        if (featureButtonPrefab == null)
        {
            Debug.LogError("FeatureButton prefab is missing.");
            return;
        }

        if (featureContainer == null)
        {
            Debug.LogError("Feature container is missing.");
            return;
        }

        foreach (Feature feature in features)
        {
            if (feature == null)
                continue;

            FeatureButton button =
                Instantiate(
                    featureButtonPrefab,
                    featureContainer
                );

            FeatureContext context =
                new FeatureContext(
                    unit,
                    city,
                    null,
                    unit != null ? unit.Owner : city.Owner
                ); ;

            button.Initialize(
                feature,
                context,
                featureSystem,
                selectionSystem
            );
        }
    }

    public void Clear()
    {
        if (featureContainer == null)
            return;

        for (int i = featureContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(featureContainer.GetChild(i).gameObject);
        }
    }
}