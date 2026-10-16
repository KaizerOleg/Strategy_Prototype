using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndTurnUI : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TMP_Text turnText;

    private void Start()
    {
        if (turnManager == null)
        {
            Debug.LogError(
                "TurnManager is missing."
            );

            return;
        }

        endTurnButton.onClick.AddListener(
            EndTurn
        );

        turnManager.OnTurnStarted += UpdateTurn;

        UpdateTurn(
            turnManager.CurrentTurn
        );
    }

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnStarted -= UpdateTurn;
        }

        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(
                EndTurn
            );
        }
    }

    public void EndTurn()
    {
        turnManager.EndTurn();
    }

    private void UpdateTurn(int turn)
    {
        if (turnText != null)
        {
            turnText.text =
                $"Turn: {turn}";
        }
    }
}