using Unity.Netcode;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData Data { get; private set; }
    public HexTile CurrentTile { get; private set; }

    public int CurrentHealth { get; private set; }
    public int MovementPoints { get; private set; }

    public EffectContainer Effects { get; private set; }

    public Player Owner { get; private set; }

    private SpriteRenderer spriteRenderer;
    private UnitMovement movement;
    private UnitMovementVisualizer movementVisualizer;

    private bool initialized;

    private void Awake()
    {
        Effects =
            GetComponent<EffectContainer>();

        if (Effects == null)
        {
            Debug.LogError(
                "EffectContainer is missing on Unit.",
                this
            );
        }

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        movement =
            GetComponent<UnitMovement>();

        movementVisualizer =
            GetComponent<UnitMovementVisualizer>();
    }

    // =========================================================
    // INITIALIZATION
    // =========================================================

    public void Initialize(
        Player owner,
        UnitData data,
        HexTile startingTile,
        HexGrid grid)
    {
        if (data == null)
        {
            Debug.LogError(
                "UnitData is null.",
                this
            );

            return;
        }

        if (owner == null)
        {
            Debug.LogError(
                "Unit owner is null.",
                this
            );

            return;
        }

        if (startingTile == null)
        {
            Debug.LogError(
                "Starting tile is null.",
                this
            );

            return;
        }

        if (grid == null)
        {
            Debug.LogError(
                "HexGrid is null.",
                this
            );

            return;
        }

        bool isFirstInitialization =
            !initialized;

        bool configurationChanged =
            Owner != owner ||
            Data != data;

        Owner = owner;
        Data = data;

        Owner.AddUnit(this);

        if (isFirstInitialization ||
            configurationChanged)
        {
            CurrentHealth =
                Data.maxHealth;

            ResetMovement();
        }

        if (Data.sprite != null &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite =
                Data.sprite;
        }

        SetCurrentTile(startingTile);

        if (movement != null)
        {
            movement.InitializePathfinding(
                grid
            );
        }

        initialized = true;
    }

    // =========================================================
    // NETWORK INITIALIZATION
    // =========================================================

    public void SetNetworkHealth(
        int value)
    {
        if (Data == null)
            return;

        CurrentHealth =
            Mathf.Clamp(
                value,
                0,
                Data.maxHealth
            );
    }

    public void SetNetworkMovementPoints(int value)
    {
        Debug.Log(
            $"[Unit] SetNetworkMovementPoints: {value}, " +
            $"Data exists: {Data != null}, Unit: {name}"
        );

        if (Data == null)
            return;

        MovementPoints = Mathf.Clamp(value, 0, Data.movementRange);
    }

    public void NetworkSetCurrentTile(
        HexTile tile)
    {
        if (tile == null)
            return;

        if (CurrentTile != null &&
            CurrentTile != tile)
        {
            CurrentTile.ClearOccupyingUnit();
        }

        CurrentTile = tile;

        tile.SetOccupyingUnit(
            this
        );

        transform.position =
            tile.transform.position;
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    public bool CanMove()
    {
        return MovementPoints > 0;
    }

    public bool CanSpendMovement(
        int movementCost)
    {
        if (movementCost < 0)
            return false;

        return MovementPoints >= movementCost;
    }

    public void SpendMovement(
        int movementCost)
    {
        if (!CanSpendMovement(movementCost))
            return;

        MovementPoints -= movementCost;
    }

    public void ResetMovement()
    {
        if (Data == null)
            return;

        MovementPoints =
            Data.movementRange;
    }

    public void MoveTo(
        HexTile targetTile,
        int movementCost)
    {
        if (targetTile == null)
            return;

        if (targetTile.IsOccupied())
            return;

        if (!CanSpendMovement(movementCost))
            return;

        if (CurrentTile != null &&
            CurrentTile != targetTile)
        {
            CurrentTile.ClearOccupyingUnit();
        }

        SpendMovement(
            movementCost
        );

        SetCurrentTile(
            targetTile
        );
    }

    private void SetCurrentTile(
        HexTile tile)
    {
        if (tile == null)
            return;

        if (CurrentTile != null &&
            CurrentTile != tile)
        {
            CurrentTile.ClearOccupyingUnit();
        }

        CurrentTile = tile;

        tile.SetOccupyingUnit(
            this
        );

        transform.position =
            tile.transform.position;
    }

    // =========================================================
    // COMBAT
    // =========================================================

    public int GetAttack()
    {
        if (Data == null)
            return 0;

        int attack =
            Data.attack;

        if (Effects == null)
            return attack;

        foreach (
            ActiveEffect effect
            in Effects.Effects)
        {
            if (effect == null)
                continue;

            if (effect.Data is NewWeaponsEffect
                weaponsEffect)
            {
                attack +=
                    weaponsEffect.GetAttackBonus(
                        effect
                    );
            }
        }

        return attack;
    }

    public void Heal(
        int amount)
    {
        if (amount <= 0)
            return;

        if (Data == null)
            return;

        CurrentHealth += amount;

        if (CurrentHealth >
            Data.maxHealth)
        {
            CurrentHealth =
                Data.maxHealth;
        }
    }

    public void TakeDamage(
        int amount)
    {
        if (amount <= 0)
            return;

        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;

            DestroyUnit();
        }
    }

    // =========================================================
    // MOVEMENT VISUALIZATION
    // =========================================================

    public void ShowMovementRange()
    {
        Debug.Log(
            $"[MovementRange] Unit: {name}, " +
            $"Tile: {CurrentTile?.Coordinates}, " +
            $"MovementPoints: {MovementPoints}"
        );

        if (movement == null || movementVisualizer == null)
        {
            Debug.LogError(
                $"[MovementRange] Missing component on {name}. " +
                $"Movement: {movement != null}, " +
                $"Visualizer: {movementVisualizer != null}"
            );

            return;
        }

        var availableTiles = movement.GetAvailableTiles();

        Debug.Log(
            $"[MovementRange] Available tiles: {availableTiles.Count}"
        );

        movementVisualizer.Show(availableTiles);
    }

    public void HideMovementRange()
    {
        if (movementVisualizer == null)
            return;

        movementVisualizer.Clear();
    }

    // =========================================================
    // DESTRUCTION
    // =========================================================

    public void DestroyUnit()
    {
        if (Owner != null)
        {
            Owner.RemoveUnit(this);
        }

        if (CurrentTile != null)
        {
            CurrentTile.ClearOccupyingUnit();
            CurrentTile = null;
        }

        NetworkObject networkObject =
            GetComponent<NetworkObject>();

        if (networkObject != null &&
            networkObject.IsSpawned)
        {
            if (networkObject.IsOwnedByServer)
            {
                networkObject.Despawn(
                    true
                );
            }

            return;
        }

        Destroy(gameObject);
    }
}