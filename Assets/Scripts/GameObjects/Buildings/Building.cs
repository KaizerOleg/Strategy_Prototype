using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData Data { get; private set; }

    public City City { get; private set; }

    public HexTile Tile { get; private set; }

    public EffectContainer Effects { get; private set; }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        Effects = GetComponent<EffectContainer>();

        if (Effects == null)
        {
            Debug.LogError(
                "EffectContainer is missing on Building."
            );
        }

        spriteRenderer =
            GetComponent<SpriteRenderer>();
    }

    public void Initialize(
        BuildingData data,
        City city,
        HexTile tile)
    {
        if (data == null)
        {
            Debug.LogError("BuildingData is null.");
            return;
        }

        if (city == null)
        {
            Debug.LogError("City is null.");
            return;
        }

        if (tile == null)
        {
            Debug.LogError("Tile is null.");
            return;
        }

        Data = data;
        City = city;
        Tile = tile;

        if (spriteRenderer != null &&
            Data.sprite != null)
        {
            spriteRenderer.sprite =
                Data.sprite;
        }

        transform.position =
            tile.transform.position;

        tile.SetBuilding(this);

        city.AddBuilding(this);
    }

    public Yield GetYield()
    {
        if (Data == null)
            return new Yield();

        return Data.yieldBonus;
    }
}