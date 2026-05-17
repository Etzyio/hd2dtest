/*
 * File: Background3D.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 3D背景视差效果节点，用于创建深度感的背景动画。
 * 根据玩家的移动，以较慢的速度移动背景，产生视差效果。
 * 
 * Key Features:
 * - 视差因子控制背景移动速度
 * - 仅在水平方向（X轴）上应用视差效果
 * - 自动跟随玩家位置变化
 */
using Godot;
using System;

namespace hd2dtest.Scripts.World
{
    /// <summary>
    /// 3D背景视差效果节点
    /// 根据玩家移动创建深度感的背景动画
    /// </summary>
    public partial class Background3D : Node3D
    {
        /// <summary>
        /// 玩家角色引用
        /// </summary>
        public CharacterBody3D Player;

        /// <summary>
        /// 视差因子（0-1之间，值越小背景移动越慢）
        /// </summary>
        public float ParallaxFactor = 0.1f;

        /// <summary>
        /// 初始位置（用于计算偏移）
        /// </summary>
        private Vector3 _initialPosition;

        /// <summary>
        /// 玩家上一帧的位置
        /// </summary>
        private Vector3 _playerLastPosition;

        /// <summary>
        /// 节点就绪时的回调
        /// 初始化初始位置和玩家位置
        /// </summary>
        public override void _Ready()
        {
            _initialPosition = Position;
            if (Player != null)
            {
                _playerLastPosition = Player.Position;
            }
        }

        /// <summary>
        /// 每帧更新方法
        /// 根据玩家移动更新背景位置，产生视差效果
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        public override void _Process(double delta)
        {
            // 如果没有玩家引用，直接返回
            if (Player == null)
                return;

            // 计算玩家自上一帧以来的位移
            Vector3 playerDelta = Player.Position - _playerLastPosition;
            
            // 计算背景位移（只在X轴上应用视差效果）
            Vector3 backgroundDelta = new(playerDelta.X * ParallaxFactor, 0, 0);

            // 更新背景位置
            Position = _initialPosition + backgroundDelta;
            
            // 记录当前玩家位置供下一帧使用
            _playerLastPosition = Player.Position;
        }
    }
}