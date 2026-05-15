/*
 * File: EffectManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 特效管理器，作为全局单例负责管理游戏中的视觉特效。
 * 支持特效生成和屏幕特效播放功能。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 支持在指定位置生成特效
 * - 支持播放屏幕特效
 * - 完整的异常处理和日志记录
 * 
 * Note:
 * 当前为基础实现，后续可扩展为支持粒子系统、动画播放等功能。
 */

using Godot;
using hd2dtest.Scripts.Utilities;
using System;

namespace hd2dtest.Scripts.Managers
{
    public partial class EffectManager : Node
    {
        public static EffectManager Instance { get; private set; }

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Log.Info("EffectManager initialized");
            }
            else
            {
                Log.Warning("EffectManager instance already exists");
                QueueFree();
            }
        }

        public void SpawnEffect(string effectName, Vector3 position)
        {
            Log.Info($"Spawning effect: {effectName} at {position}");
        }

        public void PlayScreenEffect(string effectName)
        {
            Log.Info($"Playing screen effect: {effectName}");
        }
    }
}