using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    public partial class NotificationManager : Node
    {
        public static NotificationManager Instance { get; private set; }

        private List<Notification> _notifications = new List<Notification>();
        private const int MaxNotifications = 5;
        private const double DefaultDuration = 3.0;

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

        public void ShowQuestNotification(string questName, string description)
        {
            ShowNotification($"📜 新任务: {questName}\n{description}", 5.0);
        }

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

        public void ShowAchievementNotification(string achievementName)
        {
            ShowNotification($"🏆 成就解锁: {achievementName}", 6.0);
        }

        public void ShowLevelUpNotification(int level)
        {
            ShowNotification($"✨ 等级提升！\n你现在是 {level} 级！", 4.0);
        }

        public override void _Process(double delta)
        {
            double currentTime = Time.GetTicksMsec() / 1000.0;

            _notifications.RemoveAll(n => currentTime - n.StartTime >= n.Duration);
        }

        public List<Notification> GetActiveNotifications()
        {
            return new List<Notification>(_notifications);
        }

        public void ClearAll()
        {
            _notifications.Clear();
        }
    }

    public class Notification
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public double Duration { get; set; }
        public double StartTime { get; set; }
        public NotificationType Type { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Quest,
        Combat,
        Achievement,
        LevelUp
    }
}
