using UnityEngine;

public class HexTileView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer outlineRenderer;

    [Header("Outline")]
    [SerializeField] private Color normalOutlineColor = Color.black;
    [SerializeField] private Color movementOutlineColor = Color.yellow;

    [Header("Feature Target")]
    [SerializeField] private Color featureTargetOutlineColor = Color.red;

    [Header("Build Placement")]
    [SerializeField]
    [Range(0f, 1f)]
    private float blockedBrightness = 0.3f;

    [SerializeField]
    private Color blockedColor = Color.black;

    [SerializeField]
    private float outlineScale = 1.05f;

    private Color originalTerrainColor = Color.white;

    private bool movementHighlighted;
    private bool featureTargetHighlighted;

    private void Awake()
    {
        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer is not assigned.");

        if (outlineRenderer == null)
            Debug.LogError("Outline Renderer is not assigned.");
    }

    public void SetTerrain(TerrainData terrain)
    {
        if (terrain == null)
        {
            Debug.LogWarning("TerrainData is null.");
            return;
        }

        if (terrain.sprite != null)
            spriteRenderer.sprite = terrain.sprite;

        originalTerrainColor = terrain.color;

        spriteRenderer.color = originalTerrainColor;

        outlineRenderer.sprite = spriteRenderer.sprite;
        outlineRenderer.transform.localScale =
            Vector3.one * outlineScale;

        outlineRenderer.enabled = true;

        RefreshOutline();
    }

    public void SetMovementHighlight(bool active)
    {
        movementHighlighted = active;

        RefreshOutline();
    }

    public void SetFeatureTargetHighlight(
        bool active,
        Color color)
    {
        featureTargetHighlighted = active;

        if (active)
            featureTargetOutlineColor = color;

        RefreshOutline();
    }

    private void RefreshOutline()
    {
        if (outlineRenderer == null)
            return;

        if (featureTargetHighlighted)
        {
            outlineRenderer.color =
                featureTargetOutlineColor;

            return;
        }

        if (movementHighlighted)
        {
            outlineRenderer.color =
                movementOutlineColor;

            return;
        }

        outlineRenderer.color =
            normalOutlineColor;
    }

    public void SetBuildPlacementBlocked(bool blocked)
    {
        if (blocked)
        {
            spriteRenderer.color =
                Color.Lerp(
                    originalTerrainColor,
                    blockedColor,
                    1f - blockedBrightness
                );
        }
        else
        {
            spriteRenderer.color =
                originalTerrainColor;
        }
    }
}