using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    Finished
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentState = GameState.Menu;
    }

    public void StartGame()
    {
        if (CurrentState == GameState.Playing)
            return;

        CurrentState = GameState.Playing;

        OnGameStarted?.Invoke();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Paused;

        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
            return;

        CurrentState = GameState.Playing;

        OnGameResumed?.Invoke();
    }

    public void EndGame()
    {
        if (CurrentState == GameState.Finished)
            return;

        CurrentState = GameState.Finished;

        OnGameEnded?.Invoke();
    }
}