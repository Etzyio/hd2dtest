using Godot;
using System;

namespace hd2dtest.Scripts.Core
{
    public partial class GameManager : Node
{
    // 单例实例
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    public enum GameState
    {
        Title,
        Playing,
        Paused,
        GameOver
    }
    
    private GameState _currentState;
    
    [Export]
    public Node2D Player;
    
    [Export]
    public CanvasLayer UI;
    
    public override void _Ready()
    {
        // 设置单例实例
        _instance = this;
        
        _currentState = GameState.Title;
        InitializeGame();
    }
    
    public override void _Process(double delta)
    {
        switch (_currentState)
        {
            case GameState.Title:
                HandleTitleInput();
                break;
            case GameState.Playing:
                HandlePlayingInput();
                break;
            case GameState.Paused:
                HandlePausedInput();
                break;
            case GameState.GameOver:
                HandleGameOverInput();
                break;
        }
    }
    
    private void InitializeGame()
    {
        // 初始化游戏设置
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        // 显示标题UI
        ShowUI("Title");
    }
    
    private void StartGame()
    {
        _currentState = GameState.Playing;
        ShowUI("Game");
        // 重置玩家位置和状态
        if (Player != null)
        {
            Player.Position = new Vector2(0, 0);
        }
    }
    
    private void PauseGame()
    {
        _currentState = GameState.Paused;
        ShowUI("Pause");
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    
    private void ResumeGame()
    {
        _currentState = GameState.Playing;
        ShowUI("Game");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
    
    private void GameOver()
    {
        _currentState = GameState.GameOver;
        ShowUI("GameOver");
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    
    private void ShowUI(string uiName)
    {
        // 显示指定UI，隐藏其他UI
        if (UI != null)
        {
            foreach (Node child in UI.GetChildren())
            {
                if (child is CanvasItem canvasItem)
                {
                    canvasItem.Visible = child.Name == uiName;
                }
            }
        }
    }
    
    private void HandleTitleInput()
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            StartGame();
        }
    }
    
    private void HandlePlayingInput()
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            PauseGame();
        }
    }
    
    private void HandlePausedInput()
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            ResumeGame();
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            // 返回标题
            _currentState = GameState.Title;
            ShowUI("Title");
        }
    }
    
    private void HandleGameOverInput()
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            StartGame();
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            // 返回标题
            _currentState = GameState.Title;
            ShowUI("Title");
        }
    }
    
    // 公共方法，供其他节点调用
    public void OnPlayerDeath()
    {
        GameOver();
    }
    
    public GameState GetCurrentState()
    {
        return _currentState;
    }
    }
}