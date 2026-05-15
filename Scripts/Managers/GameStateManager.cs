using Godot;
using hd2dtest.Scripts.Utilities;
using System;

namespace hd2dtest.Scripts.Managers
{
    public enum GameState
    {
        Menu,
        Loading,
        Playing,
        Battle,
        Dialogue,
        Paused,
        GameOver
    }

    public partial class GameStateManager : Node
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Menu;

        [Signal]
        public delegate void GameStateChangedEventHandler(GameState newState);

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Log.Info("GameStateManager initialized");
            }
            else
            {
                Log.Warning("GameStateManager instance already exists");
                QueueFree();
            }
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            Log.Info($"Game state changing from {CurrentState} to {newState}");
            CurrentState = newState;
            EmitSignal(SignalName.GameStateChanged, (int)newState);
        }

        public bool IsState(GameState state)
        {
            return CurrentState == state;
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }
    }
}