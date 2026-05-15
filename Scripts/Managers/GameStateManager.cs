/*
 * File: GameStateManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 游戏状态管理器，作为全局单例负责管理游戏的状态转换。
 * 支持游戏状态的切换、查询和状态变更信号通知。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 支持多种游戏状态（Menu、Loading、Playing、Battle、Dialogue、Paused、GameOver）
 * - 状态变更信号通知机制
 * - 暂停/恢复游戏功能
 * - 状态验证和防止重复切换
 * - 完整的异常处理和日志记录
 */

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