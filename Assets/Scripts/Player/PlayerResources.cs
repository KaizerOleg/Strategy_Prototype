using System;

[Serializable]
public class PlayerResources
{
    public int Food { get; private set; }
    public int Production { get; private set; }
    public int Gold { get; private set; }
    public int Science { get; private set; }

    public event Action OnResourcesChanged;

    public PlayerResources(
        int startingFood = 0,
        int startingProduction = 0,
        int startingGold = 0,
        int startingScience = 0)
    {
        Food = startingFood;
        Production = startingProduction;
        Gold = startingGold;
        Science = startingScience;
    }

    public void Add(Yield yield)
    {
        Food += yield.food;
        Production += yield.production;
        Gold += yield.gold;
        Science += yield.science;

        OnResourcesChanged?.Invoke();
    }

    public bool CanAfford(
        int productionCost,
        int goldCost)
    {
        return
            Production >= productionCost &&
            Gold >= goldCost;
    }

    public bool TrySpend(
        int productionCost,
        int goldCost)
    {
        if (productionCost < 0 ||
            goldCost < 0)
        {
            return false;
        }

        if (!CanAfford(
                productionCost,
                goldCost))
        {
            return false;
        }

        Production -= productionCost;
        Gold -= goldCost;

        OnResourcesChanged?.Invoke();

        return true;
    }
    public bool TrySpendScience(int amount)
    {
        if (amount < 0 || Science < amount)
            return false;

        Science -= amount;

        OnResourcesChanged?.Invoke();

        return true;
    }
    public void AddFood(int amount)
    {
        Food += amount;
        OnResourcesChanged?.Invoke();
    }

    public void AddProduction(int amount)
    {
        Production += amount;
        OnResourcesChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnResourcesChanged?.Invoke();
    }

    public void AddScience(int amount)
    {
        Science += amount;
        OnResourcesChanged?.Invoke();
    }

    public bool CanAffordGold(int amount)
    {
        return Gold >= amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || Gold < amount)
            return false;

        Gold -= amount;

        OnResourcesChanged?.Invoke();

        return true;
    }

    public void SetValues(
    int food,
    int production,
    int gold,
    int science)
    {
        if (Food == food &&
            Production == production &&
            Gold == gold &&
            Science == science)
        {
            return;
        }

        Food = food;
        Production = production;
        Gold = gold;
        Science = science;

        OnResourcesChanged?.Invoke();
    }
}
