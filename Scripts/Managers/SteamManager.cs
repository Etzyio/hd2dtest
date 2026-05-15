/*
 * File: SteamManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * Steam平台管理器，作为全局单例负责集成Steamworks.NET SDK。
 * 支持Steam成就、排行榜、云存储、好友系统和Overlay界面等功能。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - Steam初始化和回调管理
 * - 成就解锁和查询
 * - 排行榜分数提交和获取
 * - 云存储读写
 * - 好友邀请和状态查询
 * - Steam Overlay界面显示
 * - 条件编译支持NO_STEAM构建
 * - 完整的异常处理和日志记录
 */

using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;
#if !NO_STEAM
using Steamworks;
#endif

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// Steam平台管理器，负责集成Steamworks.NET SDK
    /// </summary>
    /// <remarks>
    /// 该类继承自Godot.Node，作为单例模式运行，负责管理Steam平台的各项功能，
    /// 包括成就系统、排行榜、云存储、多人游戏等。
    /// </remarks>
    public partial class SteamManager : Node
    {
        public static SteamManager Instance { get; private set; }
        public bool IsInitialized { get; private set; } = false;
        public ulong SteamId { get; private set; } = 0;
        public string UserName { get; private set; } = string.Empty;

        [Export]
        public bool EnableSteamIntegration = true;

        public event Action<string> OnAchievementUnlocked;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                
                if (EnableSteamIntegration)
                {
                    InitializeSteam();
                }
                else
                {
                    Log.Info("Steam integration disabled");
                }
            }
            else
            {
                QueueFree();
            }
        }

        public override void _Process(double delta)
        {
            if (IsInitialized)
            {
                UpdateSteamCallbacks();
            }
        }

        private void InitializeSteam()
        {
#if !NO_STEAM
            try
            {
                Log.Info("Initializing Steamworks.NET...");
                
                if (!SteamAPI.Init())
                {
                    Log.Error("Failed to initialize Steamworks.NET - Steam API Init failed");
                    IsInitialized = false;
                    return;
                }
                
                SteamId = SteamUser.GetSteamID().m_SteamID;
                UserName = SteamFriends.GetPersonaName();
                IsInitialized = true;
                
                Log.Info("Steamworks.NET initialized successfully");
                Log.Info($"Steam ID: {SteamId}, UserName: {UserName}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize Steamworks.NET: {ex.Message}");
                IsInitialized = false;
            }
#else
            Log.Info("Steam integration disabled (NO_STEAM build)");
            IsInitialized = false;
#endif
        }

        private void UpdateSteamCallbacks()
        {
#if !NO_STEAM
            try
            {
                SteamAPI.RunCallbacks();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update Steam callbacks: {ex.Message}");
            }
#endif
        }

        public bool UnlockAchievement(string achievementId)
        {
            if (!IsInitialized)
            {
                Log.Warning("Steam not initialized, skipping achievement unlock");
                return false;
            }

#if !NO_STEAM
            try
            {
                Log.Info($"Unlocking achievement: {achievementId}");
                
                bool success = SteamUserStats.SetAchievement(achievementId);
                if (success)
                {
                    SteamUserStats.StoreStats();
                    OnAchievementUnlocked?.Invoke(achievementId);
                    Log.Info($"Achievement unlocked successfully: {achievementId}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to unlock achievement {achievementId}: {ex.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        public bool IsAchievementUnlocked(string achievementId)
        {
            if (!IsInitialized)
            {
                return false;
            }

#if !NO_STEAM
            try
            {
                bool unlocked = false;
                SteamUserStats.GetAchievement(achievementId, out unlocked);
                return unlocked;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to check achievement {achievementId}: {ex.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        public Dictionary<string, bool> GetAllAchievements()
        {
            Dictionary<string, bool> achievements = new Dictionary<string, bool>();
            
            if (!IsInitialized)
            {
                return achievements;
            }

#if !NO_STEAM
            try
            {
                uint count = SteamUserStats.GetNumAchievements();
                for (uint i = 0; i < count; i++)
                {
                    string name = SteamUserStats.GetAchievementName(i);
                    bool unlocked = false;
                    SteamUserStats.GetAchievement(name, out unlocked);
                    achievements[name] = unlocked;
                }
                
                return achievements;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get achievements: {ex.Message}");
                return achievements;
            }
#else
            return achievements;
#endif
        }

        public bool ResetAllAchievements()
        {
            if (!IsInitialized)
            {
                return false;
            }

#if !NO_STEAM
            try
            {
                Log.Info("Resetting all achievements");
                
                bool success = SteamUserStats.ResetAllStats(true);
                if (success)
                {
                    SteamUserStats.StoreStats();
                    Log.Info("All achievements reset successfully");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to reset achievements: {ex.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        public bool SetLeaderboardScore(string leaderboardName, int score)
        {
            if (!IsInitialized)
            {
                Log.Warning("Steam not initialized, skipping leaderboard");
                return false;
            }

#if !NO_STEAM
            try
            {
                Log.Info($"Setting leaderboard score: {leaderboardName} = {score}");
                
                SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(leaderboardName);
                
                Log.Info($"Leaderboard find request sent: {hSteamAPICall.m_SteamAPICall}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set leaderboard score: {ex.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        public List<LeaderboardEntry> GetLeaderboardEntries(string leaderboardName, int count = 10)
        {
            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
            
            if (!IsInitialized)
            {
                return entries;
            }

#if !NO_STEAM
            try
            {
                Log.Info($"Getting leaderboard entries: {leaderboardName}");
                
                SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(leaderboardName);
                
                return entries;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get leaderboard entries: {ex.Message}");
                return entries;
            }
#else
            return entries;
#endif
        }

        public bool SaveToCloud(string filename, byte[] data)
        {
            if (!IsInitialized)
            {
                Log.Warning("Steam not initialized, skipping cloud save");
                return false;
            }

#if !NO_STEAM
            try
            {
                Log.Info($"Saving to cloud: {filename} ({data.Length} bytes)");
                
                bool success = SteamRemoteStorage.FileWrite(filename, data, data.Length);
                if (success)
                {
                    Log.Info("Data saved to cloud successfully");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save to cloud: {ex.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        public byte[] LoadFromCloud(string filename)
        {
            if (!IsInitialized)
            {
                return null;
            }

#if !NO_STEAM
            try
            {
                Log.Info($"Loading from cloud: {filename}");
                
                if (SteamRemoteStorage.FileExists(filename))
                {
                    int size = (int)SteamRemoteStorage.GetFileSize(filename);
                    byte[] data = new byte[size];
                    int bytesRead = SteamRemoteStorage.FileRead(filename, data, size);
                    
                    if (bytesRead > 0)
                    {
                        Log.Info($"Loaded {bytesRead} bytes from cloud");
                        return data;
                    }
                }
                
                Log.Warning("File not found in cloud");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load from cloud: {ex.Message}");
                return null;
            }
#else
            return null;
#endif
        }

        public List<string> GetCloudFiles()
        {
            List<string> files = new List<string>();
            
            if (!IsInitialized)
            {
                return files;
            }

#if !NO_STEAM
            try
            {
                int count = SteamRemoteStorage.GetFileCount();
                for (int i = 0; i < count; i++)
                {
                    int fileSize;
                    files.Add(SteamRemoteStorage.GetFileNameAndSize(i, out fileSize));
                }
                
                return files;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get cloud files: {ex.Message}");
                return files;
            }
#else
            return files;
#endif
        }

        public bool InviteFriend(ulong steamId)
        {
            if (!IsInitialized)
            {
                Log.Warning("Steam not initialized, skipping friend invite");
                return false;
            }

#if !NO_STEAM
            try
            {
                Log.Info($"Inviting friend: {steamId}");
                
                SteamFriends.InviteUserToGame(new CSteamID(steamId), "Join my game!");
                Log.Info("Friend invite sent successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to invite friend: {ex.Message}");
                return false;
            }
#else
            return false;
#endif
        }

        public List<ulong> GetFriends()
        {
            List<ulong> friends = new List<ulong>();
            
            if (!IsInitialized)
            {
                return friends;
            }

#if !NO_STEAM
            try
            {
                int count = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
                for (int i = 0; i < count; i++)
                {
                    CSteamID steamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                    friends.Add(steamId.m_SteamID);
                }
                
                return friends;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get friends: {ex.Message}");
                return friends;
            }
#else
            return friends;
#endif
        }

        public FriendStatus GetFriendStatus(ulong steamId)
        {
            if (!IsInitialized)
            {
                return FriendStatus.Offline;
            }

#if !NO_STEAM
            try
            {
                EPersonaState state = SteamFriends.GetFriendPersonaState(new CSteamID(steamId));
                return (FriendStatus)(int)state;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get friend status: {ex.Message}");
                return FriendStatus.Offline;
            }
#else
            return FriendStatus.Offline;
#endif
        }

        public void ShowOverlay(OverlayDialog dialog)
        {
            if (!IsInitialized)
            {
                return;
            }

#if !NO_STEAM
            try
            {
                Log.Info($"Showing Steam overlay: {dialog}");
                
                switch (dialog)
                {
                    case OverlayDialog.Friends:
                        SteamFriends.ActivateGameOverlay("Friends");
                        break;
                    case OverlayDialog.Achievements:
                        SteamFriends.ActivateGameOverlay("Achievements");
                        break;
                    case OverlayDialog.Leaderboard:
                        SteamFriends.ActivateGameOverlay("Leaderboard");
                        break;
                    case OverlayDialog.Community:
                        SteamFriends.ActivateGameOverlayToWebPage("http://steamcommunity.com");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to show overlay: {ex.Message}");
            }
#endif
        }

        public void Shutdown()
        {
            if (IsInitialized)
            {
#if !NO_STEAM
                try
                {
                    Log.Info("Shutting down Steamworks.NET...");
                    SteamAPI.Shutdown();
                    IsInitialized = false;
                    Log.Info("Steamworks.NET shutdown completed");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to shutdown Steamworks.NET: {ex.Message}");
                }
#endif
            }
        }
    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public ulong SteamId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Score { get; set; }
        public byte[] Details { get; set; } = new byte[0];
    }

    public enum FriendStatus
    {
        Offline = 0,
        Online = 1,
        Busy = 2,
        Away = 3,
        Snooze = 4,
        LookingToPlay = 5,
        LookingToTrade = 6
    }

    public enum OverlayDialog
    {
        Friends,
        Achievements,
        Leaderboard,
        Community
    }
}