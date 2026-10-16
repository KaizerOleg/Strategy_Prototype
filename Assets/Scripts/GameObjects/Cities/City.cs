using System;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
    public CityData Data { get; private set; }
    public Player Owner { get; private set; }
    public HexTile CenterTile { get; private set; }
    public EffectContainer Effects { get; private set; }
    public int CurrentProduction { get; private set; }

    public event Action<int> OnProductionChanged;

    public int BuildingsBuiltThisTurn { get; private set; }

    private readonly List<HexTile> territory = new();
    private readonly List<Building> buildings = new();

    public IReadOnlyList<HexTile> Territory => territory;
    public IReadOnlyList<Building> Buildings => buildings;

    public void Initialize(
        CityData data,
        Player owner,
        HexTile centerTile)
    {
        if (data == null)
        {
            Debug.LogError("CityData is null.");
            return;
        }

        if (owner == null)
        {
            Debug.LogError("City owner is null.");
            return;
        }

        if (centerTile == null)
        {
            Debug.LogError("City center tile is null.");
            return;
        }

        Effects = GetComponent<EffectContainer>();

        if (Effects == null)
        {
            Debug.LogError(
                "EffectContainer is missing on City."
            );
        }

        Data = data;
        Owner = owner;
        CenterTile = centerTile;

        BuildingsBuiltThisTurn = 0;
        CurrentProduction = 0;

        centerTile.SetCity(this);
        centerTile.SetOwner(owner);

        owner.AddCity(this);

        transform.position =
            centerTile.transform.position;
    }

    public void AddProduction(int amount)
    {
        if (amount <= 0)
            return;

        CurrentProduction += amount;
        OnProductionChanged?.Invoke(CurrentProduction);
    }

    public bool CanAffordProduction(int cost)
    {
        return CurrentProduction >= cost;
    }

    public bool TrySpendProduction(int amount)
    {
        if (amount < 0 || CurrentProduction < amount)
            return false;

        CurrentProduction -= amount;
        OnProductionChanged?.Invoke(CurrentProduction);

        return true;
    }

    public void SetProductionFromNetwork(int amount)
    {
        CurrentProduction = Mathf.Max(0, amount);
        OnProductionChanged?.Invoke(CurrentProduction);
    }
    public void SetTerritory(List<HexTile> tiles)
    {
        territory.Clear();

        if (tiles == null)
            return;

        foreach (HexTile tile in tiles)
        {
            if (tile == null)
                continue;

            territory.Add(tile);
            tile.SetOwner(Owner);
        }
    }

    public void AddBuilding(Building building)
    {
        if (building == null)
            return;

        if (buildings.Contains(building))
            return;

        buildings.Add(building);
    }

    public bool ControlsTile(HexTile tile)
    {
        return territory.Contains(tile);
    }

    public bool CanBuildThisTurn()
    {
        if (Data == null)
            return false;

        return
            BuildingsBuiltThisTurn <
            Data.maxBuildingsPerTurn;
    }

    public void RegisterBuildingBuilt()
    {
        BuildingsBuiltThisTurn++;
    }

    public void ResetTurn()
    {
        BuildingsBuiltThisTurn = 0;
    }
}
