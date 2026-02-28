using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules.SkillSystem;

namespace hd2dtest.Scripts.Modules
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
            if (_buffManager == null)
            {
                _buffManager = new BuffManager();
                _buffManager.Name = "BuffManager";
                // 这里需要添加到场景树中，实际使用时需要在合适的时机初始化
            }
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
                _buffManager = new BuffManager();
                _buffManager.Name = "BuffManager";
                parent.AddChild(_buffManager);
                // 使用Godot的日志系统
                GD.Print("BuffManager initialized and added to scene");
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
                // 使用Godot的日志系统
                GD.Print("BuffManager cleaned up");
            }
        }
    }

    /// <summary>
    /// 全局Buff管理器访问
    /// </summary>
    public static class BuffManagerInstance
    {
        private static BuffManager _instance;
        private static Node _parentNode;

        /// <summary>
        /// 获取Buff管理器实例
        /// </summary>
        public static BuffManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuffManager();
                    _instance.Name = "GlobalBuffManager";
                    
                    // 如果存在父节点，添加到场景中
                    if (_parentNode != null && GodotObject.IsInstanceValid(_parentNode))
                    {
                        _parentNode.AddChild(_instance);
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 初始化全局实例
        /// </summary>
        public static void Initialize(Node parent)
        {
            _parentNode = parent;
            // 强制创建实例
            var temp = Instance;
            // 使用Godot的日志系统
            GD.Print("Global BuffManager instance initialized");
        }

        /// <summary>
        /// 清理实例
        /// </summary>
        public static void Cleanup()
        {
            if (_instance != null)
            {
                _instance.ClearAllBuffs();
                if (GodotObject.IsInstanceValid(_instance))
                {
                    _instance.QueueFree();
                }
                _instance = null;
                _parentNode = null;
                // 使用Godot的日志系统
                GD.Print("Global BuffManager instance cleaned up");
            }
        }

        /// <summary>
        /// 检查实例是否有效
        /// </summary>
        private static bool IsInstanceValid(GodotObject obj)
        {
            return obj != null && GodotObject.IsInstanceValid(obj);
        }
    }
}