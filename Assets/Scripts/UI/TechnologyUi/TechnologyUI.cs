using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechnologyUI : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private GameObject window;

    [Header("Content")]
    [SerializeField] private Transform content;

    [Header("Technology Button")]
    [SerializeField] private TechnologyButton technologyButtonPrefab;

    [Header("Systems")]
    [SerializeField] private TechnologySystem technologySystem;
    [SerializeField] private TurnManager turnManager;

    private readonly List<TechnologyButton> buttons = new();

    private void Start()
    {
        if (technologySystem == null)
        {
            Debug.LogError(
                "TechnologySystem is missing."
            );

            return;
        }

        if (turnManager == null)
        {
            Debug.LogError(
                "TurnManager is missing."
            );

            return;
        }

        technologySystem.OnTechnologyResearched +=
            OnTechnologyResearched;

        turnManager.OnTurnStarted +=
            OnTurnStarted;

        if (window != null)
            window.SetActive(false);
    }

    private void OnDestroy()
    {
        if (technologySystem != null)
        {
            technologySystem.OnTechnologyResearched -=
                OnTechnologyResearched;
        }

        if (turnManager != null)
        {
            turnManager.OnTurnStarted -=
                OnTurnStarted;
        }
    }

    private void OnTechnologyResearched(
        Player player,
        TechnologyData technology)
    {
        if (turnManager == null)
            return;

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
            return;

        // ќновлюЇмо UI т≥льки дл€ поточного гравц€.
        if (player != currentPlayer)
            return;

        Refresh();
    }

    private void OnTurnStarted(int turn)
    {
        Refresh();
    }

    public void Open()
    {
        if (window == null)
            return;

        if (turnManager == null)
            return;

        if (turnManager.CurrentPlayer == null)
            return;

        window.SetActive(true);

        GenerateTechnologyButtons();
    }

    public void Close()
    {
        if (window == null)
            return;

        window.SetActive(false);
    }

    public void Refresh()
    {
        if (window == null)
            return;

        if (!window.activeSelf)
            return;

        GenerateTechnologyButtons();
    }

    private void GenerateTechnologyButtons()
    {
        ClearButtons();

        if (technologySystem == null)
        {
            Debug.LogError(
                "TechnologySystem is missing."
            );

            return;
        }

        if (turnManager == null)
        {
            Debug.LogError(
                "TurnManager is missing."
            );

            return;
        }

        Player currentPlayer =
            turnManager.CurrentPlayer;

        if (currentPlayer == null)
        {
            Debug.LogError(
                "CurrentPlayer is missing."
            );

            return;
        }

        if (technologyButtonPrefab == null)
        {
            Debug.LogError(
                "TechnologyButton prefab is missing."
            );

            return;
        }

        if (content == null)
        {
            Debug.LogError(
                "Technology content is missing."
            );

            return;
        }

        IReadOnlyList<TechnologyData> technologies =
            technologySystem.GetTechnologies();

        foreach (
            TechnologyData technology
            in technologies)
        {
            if (technology == null)
                continue;

            CreateTechnologyButton(
                technology
            );
        }
    }

    private void CreateTechnologyButton(
        TechnologyData technology)
    {
        TechnologyButton button =
            Instantiate(
                technologyButtonPrefab,
                content
            );

        button.Initialize(
            technology,
            technologySystem,
            this,
            turnManager
        );

        buttons.Add(button);
    }

    public void HighlightRequirements(
        TechnologyData[] requirements)
    {
        if (requirements == null ||
            requirements.Length == 0)
        {
            return;
        }

        foreach (
            TechnologyData requirement
            in requirements)
        {
            if (requirement == null)
                continue;

            TechnologyButton button =
                FindButton(requirement);

            if (button != null)
            {
                StartCoroutine(
                    HighlightForSeconds(
                        button,
                        3f
                    )
                );
            }
        }
    }

    private IEnumerator HighlightForSeconds(
        TechnologyButton button,
        float duration)
    {
        if (button == null)
            yield break;

        float elapsed = 0f;

        button.SetHighlighted(true);

        while (elapsed < duration)
        {
            if (button == null)
                yield break;

            elapsed += Time.deltaTime;

            float alpha =
                Mathf.Lerp(
                    0.3f,
                    1f,
                    (Mathf.Sin(elapsed * 8f) + 1f) / 2f
                );

            button.SetHighlightAlpha(alpha);

            yield return null;
        }

        if (button != null)
        {
            button.SetHighlighted(false);
        }
    }

    private TechnologyButton FindButton(
        TechnologyData technology)
    {
        if (technology == null)
            return null;

        foreach (
            TechnologyButton button
            in buttons)
        {
            if (button == null)
                continue;

            if (button.Technology == technology)
                return button;
        }

        return null;
    }

    private void ClearButtons()
    {
        foreach (
            TechnologyButton button
            in buttons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        buttons.Clear();
    }
}