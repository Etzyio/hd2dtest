using Godot;
using System;

namespace hd2dtest.Scripts.World
{
    public partial class Background3D : Node3D
{
    [Export]
    public Node2D Player;
    
    [Export]
    public float ParallaxFactor = 0.1f;
    
    private Vector3 _initialPosition;
    private Vector2 _playerLastPosition;
    
    public override void _Ready()
    {
        _initialPosition = Position;
        if (Player != null)
        {
            _playerLastPosition = Player.Position;
        }
    }
    
    public override void _Process(double delta)
    {
        if (Player == null)
            return;
        
        Vector2 playerDelta = Player.Position - _playerLastPosition;
        Vector3 backgroundDelta = new(playerDelta.X * ParallaxFactor, 0, 0);
        
        Position = _initialPosition + backgroundDelta;
        _playerLastPosition = Player.Position;
    }
}
}