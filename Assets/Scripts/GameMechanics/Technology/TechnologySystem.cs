
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TechnologySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;

    public event Action<Player, TechnologyData>
        OnTechnologyResearched;

    public IReadOnlyList<TechnologyData> GetTechnologies()
    {
        if (turnManager == null ||
            turnManager.CurrentPlayer == null)
        {
            Debug.LogError(
                "[TechnologySystem] CurrentPlayer is missing.");
            return new List<TechnologyData>();
        }

        CivilizationData civilization =
            turnManager.CurrentPlayer.GetCivilization();

        if (civilization == null ||
            civilization.technologyDatabase == null)
        {
            Debug.LogError(
                "[TechnologySystem] TechnologyDatabase is missing.");
            return new List<TechnologyData>();
        }

        return civilization.technologyDatabase.Technologies;
    }

    public bool HasTechnology(
        Player player,
        TechnologyData technology)
    {
        return player != null &&
               technology != null &&
               player.HasTechnology(technology);
    }

    public bool HasRequiredTechnology(
        Player player,
        TechnologyData requiredTechnology)
    {
        if (requiredTechnology == null)
            return true;

        return HasTechnology(player, requiredTechnology);
    }

    public bool CanResearch(
        Player player,
        TechnologyData technology)
    {
        if (player == null || technology == null)
            return false;

        if (HasTechnology(player, technology))
            return false;

        if (player.Resources == null)
            return false;

        if (technology.requiredTechnologies != null)
        {
            foreach (TechnologyData required
                     in technology.requiredTechnologies)
            {
                if (required != null &&
                    !HasTechnology(player, required))
                {
                    return false;
                }
            }
        }

        return player.Resources.Science >=
               technology.researchCost;
    }

    public bool Research(
        Player player,
        TechnologyData technology)
    {
        if (player == null || technology == null)
            return false;

        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager != null &&
            networkManager.IsListening)
        {
            NetworkPlayer[] networkPlayers =
                FindObjectsByType<NetworkPlayer>(
                    FindObjectsSortMode.None);

            foreach (NetworkPlayer networkPlayer
                     in networkPlayers)
            {
                if (networkPlayer == null ||
                    !networkPlayer.IsSpawned ||
                    !networkPlayer.IsOwner)
                    continue;

                if (networkPlayer.PlayerId.Value != player.PlayerId)
                    continue;

                int technologyId =
                    GetTechnologyId(player, technology);

                if (technologyId < 0)
                {
                    Debug.LogError(
                        "[TechnologySystem] Technology was not found " +
                        "in the player's TechnologyDatabase.");
                    return false;
                }

                return networkPlayer.RequestResearchTechnology(
                    technologyId);
            }

            Debug.LogWarning(
                "[TechnologySystem] No locally owned NetworkPlayer " +
                $"was found for PlayerId={player.PlayerId}.");

            return false;
        }

        return ResearchAuthoritative(player, technology);
    }

    public bool ResearchAuthoritative(
        Player player,
        TechnologyData technology)
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager != null &&
            networkManager.IsListening &&
            !networkManager.IsServer)
        {
            Debug.LogError(
                "[TechnologySystem] ResearchAuthoritative " +
                "can only run on the server.");
            return false;
        }

        if (!CanResearch(player, technology))
            return false;

        if (!player.Resources.TrySpendScience(
                technology.researchCost))
        {
            return false;
        }

        player.AddTechnology(technology);

        OnTechnologyResearched?.Invoke(
            player,
            technology);

        return true;
    }

    public void NotifyTechnologySynchronized(
        Player player,
        TechnologyData technology)
    {
        if (player == null || technology == null)
            return;

        OnTechnologyResearched?.Invoke(
            player,
            technology);
    }

    private int GetTechnologyId(
        Player player,
        TechnologyData technology)
    {
        if (player == null || technology == null)
            return -1;

        CivilizationData civilization =
            player.GetCivilization();

        if (civilization == null ||
            civilization.technologyDatabase == null)
            return -1;

        var technologies =
            civilization.technologyDatabase.Technologies;

        for (int i = 0; i < technologies.Count; i++)
        {
            if (technologies[i] == technology)
                return i;
        }

        return -1;
    }
}
