using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private int playerId;

    public int PlayerId => playerId;

    public PlayerResources Resources { get; private set; }

    [Header("Civilization")]
    [SerializeField] private CivilizationData civilization;

    [Header("Starting Resources")]
    [SerializeField] private int startingFood = 10;
    [SerializeField] private int startingProduction = 10;
    [SerializeField] private int startingGold = 100;
    [SerializeField] private int startingScience = 0;

    private readonly List<City> cities = new();
    private readonly List<Unit> units = new();

    private readonly HashSet<TechnologyData>
        researchedTechnologies = new();

    public IReadOnlyList<City> Cities => cities;
    public IReadOnlyList<Unit> Units => units;

    private void Awake()
    {
        InitializeResources();
    }

    public int IndexOf()
    {
        return playerId;
    }

    public void Initialize(int id)
    {
        playerId = id;

        if (Resources == null)
        {
            InitializeResources();
        }
    }

    private void InitializeResources()
    {
        Resources = new PlayerResources(
            startingFood,
            startingProduction,
            startingGold,
            startingScience
        );
    }

    public void ResetState()
    {
        researchedTechnologies.Clear();

        // Не замінюємо PlayerResources новим об'єктом,
        // оскільки NetworkPlayer та EconomyUI підписані
        // на OnResourcesChanged поточного екземпляра.
        if (Resources == null)
        {
            InitializeResources();
        }
        else
        {
            Resources.SetValues(
                startingFood,
                startingProduction,
                startingGold,
                startingScience
            );
        }

        cities.Clear();
        units.Clear();
    }

    public void ClearTechnologies()
    {
        researchedTechnologies.Clear();
    }

    public CivilizationData GetCivilization()
    {
        return civilization;
    }

    public IReadOnlyCollection<TechnologyData> ResearchedTechnologies =>
    researchedTechnologies;

    public bool HasTechnology(
        TechnologyData technology)
    {
        if (technology == null)
            return false;

        return researchedTechnologies.Contains(
            technology
        );
    }

    public void AddTechnology(
        TechnologyData technology)
    {
        if (technology == null)
            return;

        researchedTechnologies.Add(
            technology
        );
    }

    public void AddUnit(
        Unit unit)
    {
        if (unit == null)
            return;

        if (units.Contains(unit))
            return;

        units.Add(unit);
    }

    public void RemoveUnit(
        Unit unit)
    {
        if (unit == null)
            return;

        units.Remove(unit);
    }

    public void AddCity(
        City city)
    {
        if (city == null)
            return;

        if (cities.Contains(city))
            return;

        cities.Add(city);
    }

    public void RemoveCity(
        City city)
    {
        if (city == null)
            return;

        cities.Remove(city);
    }
}