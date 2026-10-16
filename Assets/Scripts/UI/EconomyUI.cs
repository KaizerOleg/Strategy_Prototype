using TMPro;
using UnityEngine;

public class EconomyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private PlayerManager playerManager;

    [Header("Resource Text")]
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text productionText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text scienceText;

    private Player currentPlayer;

    private void Start()
    {
        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        if (playerManager == null)
        {
            Debug.LogError("[EconomyUI] PlayerManager is missing.");
            return;
        }

        // Показуємо ресурси локального гравця,
        // а не гравця, чий зараз хід.
        RefreshPlayer();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayer();
    }

    private void Update()
    {
        // Локальний гравець може призначатися після Start()
        // під час мережевої ініціалізації.
        if (currentPlayer == null ||
            currentPlayer != playerManager?.LocalPlayer)
        {
            RefreshPlayer();
        }
    }

    private void RefreshPlayer()
    {
        if (playerManager == null)
            return;

        Player localPlayer = playerManager.LocalPlayer;

        if (currentPlayer == localPlayer)
        {
            UpdateUI();
            return;
        }

        UnsubscribeFromPlayer();

        currentPlayer = localPlayer;

        if (currentPlayer == null)
        {
            ClearUI();
            return;
        }

        SubscribeToPlayer();
        UpdateUI();
    }

    private void SubscribeToPlayer()
    {
        if (currentPlayer?.Resources == null)
        {
            Debug.LogWarning(
                "[EconomyUI] Local player resources are not initialized."
            );
            return;
        }

        currentPlayer.Resources.OnResourcesChanged += UpdateUI;
    }

    private void UnsubscribeFromPlayer()
    {
        if (currentPlayer?.Resources != null)
        {
            currentPlayer.Resources.OnResourcesChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        if (currentPlayer?.Resources == null)
        {
            ClearUI();
            return;
        }

        if (foodText != null)
            foodText.text = $"Food: {currentPlayer.Resources.Food}";

        if (productionText != null)
            productionText.text = $"Production: {currentPlayer.Resources.Production}";

        if (goldText != null)
            goldText.text = $"Gold: {currentPlayer.Resources.Gold}";

        if (scienceText != null)
            scienceText.text = $"Science: {currentPlayer.Resources.Science}";
    }

    private void ClearUI()
    {
        if (foodText != null)
            foodText.text = "Food: 0";

        if (productionText != null)
            productionText.text = "Production: 0";

        if (goldText != null)
            goldText.text = "Gold: 0";

        if (scienceText != null)
            scienceText.text = "Science: 0";
    }
}