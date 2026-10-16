using UnityEngine;

[CreateAssetMenu(
    fileName = "NewTechnologyFeature",
    menuName = "Game/Technology Feature"
)]
public class TechnologyFeatureData : ScriptableObject
{
    [Header("Basic Info")]
    public string featureName;

    [TextArea]
    public string description;

    public Sprite icon;
}