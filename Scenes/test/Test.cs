using Godot;
using hd2dtest.Scripts.Player;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Utilities;

public partial class Test : Node2D
{
    private Player _player;
    private const float BOUNDARY_LIMIT = 254f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Visible = false;
        // 获取Player节点
        _player = GetNode<Player>("SubViewportContainer/SubViewport/Player");
        if (_player == null)
        {
            Log.Error("Player node not found!");
        }

        // 场景就绪，触发信号显示场景层
        Log.Info("Test scene ready, triggered SceneReady signal");
        GameViewManager.TriggerSceneReady();

        Visible = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_player == null)
        {
            return;
        }

        // 检查并处理边界限制
        CheckBoundaryLimits();
    }

    // 检查边界限制并禁用相应方向
    private void CheckBoundaryLimits()
    {
        // 获取玩家位置
        Vector3 playerPos = _player.Position;
        // 检查X轴边界
        if (playerPos.X >= BOUNDARY_LIMIT)
        {
            // 接近右边界，禁用向右移动
            _player.DisableDirection(3); // 3 = right
        }
        else if (playerPos.X <= -BOUNDARY_LIMIT)
        {
            // 接近左边界，禁用向左移动
            _player.DisableDirection(2); // 2 = left
        }
        else
        {
            // 在安全区域，启用左右移动
            _player.EnableDirection(2); // 2 = left
            _player.EnableDirection(3); // 3 = right
        }

        // 检查Y轴边界
        if (playerPos.Z >= BOUNDARY_LIMIT)
        {
            // 接近上边界，禁用向上移动
            _player.DisableDirection(0); // 0 = up
        }
        else if (playerPos.Z <= -BOUNDARY_LIMIT)
        {
            // 接近下边界，禁用向下移动
            _player.DisableDirection(1); // 1 = down
        }
        else
        {
            // 在安全区域，启用上下移动
            _player.EnableDirection(0); // 0 = up
            _player.EnableDirection(1); // 1 = down
        }
    }
}
