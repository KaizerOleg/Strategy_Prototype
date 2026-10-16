using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitProductionPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("UI")]
    [SerializeField] private Transform content;
    [SerializeField] private Button unitButtonPrefab;

    [Header("Systems")]
    [SerializeField] private UnitProductionSystem productionSystem;
    [SerializeField] private TurnManager turnManager;

    private City selectedCity;
    private NetworkCity selectedNetworkCity;

    public void Open(City city)
    {
        if (city == null)
            return;

        if (productionSystem == null || turnManager == null)
        {
            Debug.LogError(
                "UnitProductionSystem or TurnManager is missing.");
            return;
        }

        Player currentPlayer = turnManager.CurrentPlayer;

        if (currentPlayer == null || city.Owner != currentPlayer)
            return;

        NetworkCity networkCity =
            city.GetComponent<NetworkCity>();

        if (networkCity == null || !networkCity.IsSpawned)
        {
            Debug.LogError(
                "Selected city has no spawned NetworkCity.");
            return;
        }

        UnsubscribeFromCity();

        selectedCity = city;
        selectedNetworkCity = networkCity;

        selectedCity.OnProductionChanged += OnProductionChanged;

        if (panel != null)
            panel.SetActive(true);

        GenerateUnitButtons();
    }

    private void GenerateUnitButtons()
    {
        ClearButtons();

        if (selectedCity == null ||
            selectedNetworkCity == null ||
            turnManager == null)
        {
            return;
        }

        Player player = turnManager.CurrentPlayer;

        if (player == null || selectedCity.Owner != player)
            return;

        IReadOnlyList<UnitData> units =
            productionSystem.GetAvailableUnits(selectedCity);

        for (int i = 0; i < units.Count; i++)
        {
            UnitData unitData = units[i];

            if (unitData == null)
                continue;

            CreateUnitButton(player, unitData, i);
        }
    }

    private void CreateUnitButton(
        Player player,
        UnitData unitData,
        int unitTypeId)
    {
        if (unitButtonPrefab == null || content == null)
        {
            Debug.LogError(
                "Unit button prefab or content is missing.");
            return;
        }

        Button button = Instantiate(unitButtonPrefab, content);

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text =
                $"{unitData.unitName} ({unitData.productionCost})";
        }

        button.interactable =
            productionSystem.CanProduce(
                player,
                selectedCity,
                unitData);

        button.onClick.AddListener(() =>
        {
            ProduceUnit(player, unitTypeId);
        });
    }

    private void ProduceUnit(Player player, int unitTypeId)
    {
        if (selectedCity == null ||
            selectedNetworkCity == null ||
            turnManager == null)
        {
            return;
        }

        if (turnManager.CurrentPlayer != player ||
            selectedCity.Owner != player)
        {
            return;
        }

        // Запит надсилається серверу через NetworkCity.
        bool requestSent =
            selectedNetworkCity.RequestProduceUnit(unitTypeId);

        if (requestSent && selectedNetworkCity.IsServer)
        {
            GenerateUnitButtons();
        }
    }

    private void OnProductionChanged(int amount)
    {
        if (selectedCity == null)
            return;

        GenerateUnitButtons();
    }

    private void UnsubscribeFromCity()
    {
        if (selectedCity != null)
        {
            selectedCity.OnProductionChanged -=
                OnProductionChanged;
        }
    }

    private void ClearButtons()
    {
        if (content == null)
            return;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    public void Close()
    {
        UnsubscribeFromCity();

        selectedCity = null;
        selectedNetworkCity = null;

        ClearButtons();

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnDisable()
    {
        UnsubscribeFromCity();
    }
}