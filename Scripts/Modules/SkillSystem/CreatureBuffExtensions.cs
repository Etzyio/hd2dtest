using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    /// <summary>
    /// 生物Buff管理扩展
    /// </summary>
    public static class CreatureBuffExtensions
    {
        /// <summary>
        /// Buff管理器实例
        /// </summary>
        private static BuffManager _buffManager;

        /// <summary>
        /// 获取或创建Buff管理器
        /// </summary>
        private static BuffManager GetBuffManager()
        {
            _buffManager ??= new BuffManager
                {
                    Name = "BuffManager"
                };
            return _buffManager;
        }

        /// <summary>
        /// 添加Buff
        /// </summary>
        public static bool AddBuff(this Creature creature, string buffId, Creature applier = null)
        {
            var manager = GetBuffManager();
            return manager.ApplyBuff(buffId, applier ?? creature, creature);
        }

        /// <summary>
        /// 移除Buff
        /// </summary>
        public static bool RemoveBuff(this Creature creature, string buffId)
        {
            var manager = GetBuffManager();
            return manager.RemoveBuff(buffId, creature);
        }

        /// <summary>
        /// 驱散Buff
        /// </summary>
        public static int DispelBuffs(this Creature creature, bool positiveOnly = false)
        {
            var manager = GetBuffManager();
            return manager.DispelBuffs(creature, positiveOnly);
        }

        /// <summary>
        /// 获取活跃Buff
        /// </summary>
        public static List<BuffInstance> GetActiveBuffs(this Creature creature)
        {
            var manager = GetBuffManager();
            return manager.GetActiveBuffs(creature);
        }

        /// <summary>
        /// 获取Buff叠加层数
        /// </summary>
        public static int GetBuffStacks(this Creature creature, string buffId)
        {
            var manager = GetBuffManager();
            return manager.GetBuffStacks(buffId, creature);
        }

        /// <summary>
        /// 检查是否有特定Buff
        /// </summary>
        public static bool HasBuff(this Creature creature, string buffId)
        {
            return creature.GetBuffStacks(buffId) > 0;
        }

        /// <summary>
        /// 初始化Buff管理器（需要在场景初始化时调用）
        /// </summary>
        public static void InitializeBuffManager(Node parent)
        {
            if (_buffManager == null)
            {
                _buffManager = new BuffManager
                {
                    Name = "BuffManager"
                };
                parent.AddChild(_buffManager);
                // 使用日志系统
                Log.Info("BuffManager initialized and added to scene");
            }
        }

        /// <summary>
        /// 清理Buff管理器
        /// </summary>
        public static void CleanupBuffManager()
        {
            if (_buffManager != null)
            {
                _buffManager.ClearAllBuffs();
                _buffManager.QueueFree();
                _buffManager = null;
                // 使用日志系统
                Log.Info("BuffManager cleaned up");
            }
        }
    }
}