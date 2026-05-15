/*
 * File: NotificationManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 通知管理器，负责管理游戏中的各种通知消息显示。
 * 支持任务通知、战斗通知、成就通知和等级提升通知等。
 * 
 * Key Features:
 * - 单例模式，全局访问
 * - 支持多种通知类型（信息、任务、战斗、成就、等级提升）
 * - 自动过期清理机制
 * - 最大通知数量限制
 */
using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    /// <summary>
    /// 通知管理器
    /// 负责管理游戏中的各种通知消息显示
    /// </summary>
    public partial class NotificationManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static NotificationManager Instance { get; private set; }

        /// <summary>
        /// 活动通知列表
        /// </summary>
        private List<Notification> _notifications = new List<Notification>();

        /// <summary>
        /// 最大通知数量限制
        /// </summary>
        private const int MaxNotifications = 5;

        /// <summary>
        /// 默认通知持续时间（秒）
        /// </summary>
        private const double DefaultDuration = 3.0;

        /// <summary>
        /// 节点就绪时的回调
        /// 初始化单例实例
        /// </summary>
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Log.Info("NotificationManager initialized");
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// 显示普通通知消息
        /// </summary>
        /// <param name="message">通知消息内容</param>
        /// <param name="duration">持续时间（秒），默认为默认值</param>
        public void ShowNotification(string message, double duration = -1)
        {
            if (duration < 0)
            {
                duration = DefaultDuration;
            }

            Notification notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
                Duration = duration,
                StartTime = Time.GetTicksMsec() / 1000.0,
                Type = NotificationType.Info
            };

            _notifications.Add(notification);

            if (_notifications.Count > MaxNotifications)
            {
                _notifications.RemoveAt(0);
            }

            Log.Info($"Notification: {message}");
        }

        /// <summary>
        /// 显示任务通知
        /// </summary>
        /// <param name="questName">任务名称</param>
        /// <param name="description">任务描述</param>
        public void ShowQuestNotification(string questName, string description)
        {
            ShowNotification($"📜 新任务: {questName}\n{description}", 5.0);
        }

        /// <summary>
        /// 显示战斗通知
        /// </summary>
        /// <param name="message">战斗消息内容</param>
        public void ShowCombatNotification(string message)
        {
            Notification notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
                Duration = 2.0,
                StartTime = Time.GetTicksMsec() / 1000.0,
                Type = NotificationType.Combat
            };

            _notifications.Add(notification);

            if (_notifications.Count > MaxNotifications)
            {
                _notifications.RemoveAt(0);
            }
        }

        /// <summary>
        /// 显示成就通知
        /// </summary>
        /// <param name="achievementName">成就名称</param>
        public void ShowAchievementNotification(string achievementName)
        {
            ShowNotification($"🏆 成就解锁: {achievementName}", 6.0);
        }

        /// <summary>
        /// 显示等级提升通知
        /// </summary>
        /// <param name="level">新等级</param>
        public void ShowLevelUpNotification(int level)
        {
            ShowNotification($"✨ 等级提升！\n你现在是 {level} 级！", 4.0);
        }

        /// <summary>
        /// 每帧更新方法
        /// 清理过期的通知
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        public override void _Process(double delta)
        {
            double currentTime = Time.GetTicksMsec() / 1000.0;

            // 移除已过期的通知
            _notifications.RemoveAll(n => currentTime - n.StartTime >= n.Duration);
        }

        /// <summary>
        /// 获取所有活动通知
        /// </summary>
        /// <returns>活动通知列表的副本</returns>
        public List<Notification> GetActiveNotifications()
        {
            return new List<Notification>(_notifications);
        }

        /// <summary>
        /// 清空所有通知
        /// </summary>
        public void ClearAll()
        {
            _notifications.Clear();
        }
    }

    /// <summary>
    /// 通知数据类
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// 通知唯一ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 通知消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 持续时间（秒）
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// 开始时间（秒）
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// 通知类型
        /// </summary>
        public NotificationType Type { get; set; }
    }

    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 普通信息通知
        /// </summary>
        Info,

        /// <summary>
        /// 任务通知
        /// </summary>
        Quest,

        /// <summary>
        /// 战斗通知
        /// </summary>
        Combat,

        /// <summary>
        /// 成就通知
        /// </summary>
        Achievement,

        /// <summary>
        /// 等级提升通知
        /// </summary>
        LevelUp
    }
}
