/*
 * File: BuffSystem.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * Buff管理器，管理所有活跃Buff的生命周期。
 * 提供全局Buff管理器静态访问。
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    /// <summary>
    /// Buff管理器
    /// </summary>
    public partial class BuffManager : Node
    {
        private Dictionary<string, BuffData> _buffTemplates = [];
        private List<BuffInstance> _activeBuffs = [];

        /// <summary>
        /// Buff更新信号
        /// </summary>
        [Signal] public delegate void BuffAppliedEventHandler(string buffId, Creature target);
        [Signal] public delegate void BuffExpiredEventHandler(string buffId, Creature target);
        [Signal] public delegate void BuffStackedEventHandler(string buffId, Creature target, int stacks);

        /// <summary>
        /// 注册Buff模板
        /// </summary>
        public void RegisterBuffTemplate(BuffData buffData)
        {
            if (buffData != null && !string.IsNullOrEmpty(buffData.BuffId))
            {
                _buffTemplates[buffData.BuffId] = buffData;
                // 使用日志系统
                Log.Info($"Registered buff template: {buffData.BuffName} ({buffData.BuffId})");
            }
        }

        /// <summary>
        /// 获取Buff模板
        /// </summary>
        public BuffData GetBuffTemplate(string buffId)
        {
            return _buffTemplates.TryGetValue(buffId, out var template) ? template : null;
        }

        /// <summary>
        /// 应用Buff
        /// </summary>
        public bool ApplyBuff(string buffId, Creature applier, Creature target)
        {
            var template = GetBuffTemplate(buffId);
            if (template == null)
            {
                // 使用日志系统
                Log.Error($"Buff template not found: {buffId}");
                return false;
            }

            // 检查是否已存在相同Buff
            var existingBuff = _activeBuffs.FirstOrDefault(b =>
                b.Data.BuffId == buffId && b.Target == target);

            if (existingBuff != null)
            {
                // 尝试叠加
                if (existingBuff.Stack())
                {
                    // 使用日志系统
                    Log.Info($"Buff stacked: {template.BuffName} on {target.CreatureName} (stacks: {existingBuff.CurrentStacks})");
                    EmitSignal(SignalName.BuffStacked, buffId, target, existingBuff.CurrentStacks);
                }
                else
                {
                    // 使用Godot的日志系统
                    Log.Info($"Buff refreshed: {template.BuffName} on {target.CreatureName}");
                }
                return true;
            }

            // 创建新Buff实例
            var newBuff = new BuffInstance(template, applier, target);
            _activeBuffs.Add(newBuff);

            // 应用效果
            ApplyBuffEffects(newBuff);

            // 使用日志系统
            Log.Info($"Buff applied: {template.BuffName} to {target.CreatureName}");
            EmitSignal(SignalName.BuffApplied, buffId, target);
            return true;
        }

        /// <summary>
        /// 移除Buff
        /// </summary>
        public bool RemoveBuff(string buffId, Creature target)
        {
            var buff = _activeBuffs.FirstOrDefault(b =>
                b.Data.BuffId == buffId && b.Target == target);

            if (buff != null)
            {
                RemoveBuffInstance(buff);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 驱散目标的所有可驱散Buff
        /// </summary>
        public int DispelBuffs(Creature target, bool positiveOnly = false)
        {
            var buffsToRemove = _activeBuffs.Where(b =>
                b.Target == target &&
                b.Data.CanBeDispelled &&
                (!positiveOnly || b.Data.IsPositive)).ToList();

            foreach (var buff in buffsToRemove)
            {
                RemoveBuffInstance(buff);
            }

            return buffsToRemove.Count;
        }

        /// <summary>
        /// 获取目标的活跃Buff
        /// </summary>
        public List<BuffInstance> GetActiveBuffs(Creature target)
        {
            return [.. _activeBuffs.Where(b => b.Target == target)];
        }

        /// <summary>
        /// 获取特定Buff的叠加层数
        /// </summary>
        public int GetBuffStacks(string buffId, Creature target)
        {
            var buff = _activeBuffs.FirstOrDefault(b =>
                b.Data.BuffId == buffId && b.Target == target);
            return buff?.CurrentStacks ?? 0;
        }

        /// <summary>
        /// 更新所有Buff
        /// </summary>
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            var expiredBuffs = new List<BuffInstance>();

            foreach (var buff in _activeBuffs)
            {
                buff.Update(dt);

                if (buff.IsExpired)
                {
                    expiredBuffs.Add(buff);
                }
            }

            // 移除过期的Buff
            foreach (var buff in expiredBuffs)
            {
                RemoveBuffInstance(buff);
            }
        }

        /// <summary>
        /// 更新回合（用于回合制Buff）
        /// </summary>
        public void UpdateTurns()
        {
            var expiredBuffs = new List<BuffInstance>();

            foreach (var buff in _activeBuffs)
            {
                buff.UpdateTurn();

                if (buff.IsExpired)
                {
                    expiredBuffs.Add(buff);
                }
            }

            // 移除过期的Buff
            foreach (var buff in expiredBuffs)
            {
                RemoveBuffInstance(buff);
            }
        }

        /// <summary>
        /// 应用Buff效果
        /// </summary>
        private void ApplyBuffEffects(BuffInstance buff)
        {
            if (buff.Target == null || buff.Data.Effects == null) return;

            foreach (var effect in buff.Data.Effects)
            {
                ApplyEffect(buff, effect);
            }
        }

        /// <summary>
        /// 移除Buff效果
        /// </summary>
        private void RemoveBuffEffects(BuffInstance buff)
        {
            if (buff.Target == null || buff.Data.Effects == null) return;

            foreach (var effect in buff.Data.Effects)
            {
                RemoveEffect(buff, effect);
            }
        }

        /// <summary>
        /// 应用单个效果
        /// </summary>
        private void ApplyEffect(BuffInstance buff, BuffEffect effect)
        {
            if (buff.Target == null) return;

            float value = effect.Value * buff.CurrentStacks;
            var target = buff.Target;

            switch (effect.EffectType)
            {
                case BuffEffectType.AttackBonus:
                    if (effect.IsPercentage)
                        target.Attack *= (1f + value * 0.01f);
                    else
                        target.Attack += value;
                    break;

                case BuffEffectType.DefenseBonus:
                    if (effect.IsPercentage)
                        target.Defense *= (1f + value * 0.01f);
                    else
                        target.Defense += value;
                    break;

                case BuffEffectType.SpeedBonus:
                    if (effect.IsPercentage)
                        target.Speed *= (1f + value * 0.01f);
                    else
                        target.Speed += value;
                    break;

                case BuffEffectType.HealthRegeneration:
                    target.Health = Math.Min(target.MaxHealth, target.Health + value);
                    break;

                case BuffEffectType.ManaRegeneration:
                    // 检查目标是否为玩家，只有玩家有魔法值
                    if (target is Player player)
                    {
                        player.Mana = Math.Min(player.MaxMana, player.Mana + value);
                    }
                    break;

                case BuffEffectType.DamageReduction:
                    // 这里可以实现伤害减免逻辑
                    break;

                case BuffEffectType.CriticalRateBonus:
                    // 这里可以实现暴击率加成逻辑
                    break;

                case BuffEffectType.StatusResistance:
                    // 这里可以实现状态抗性逻辑
                    break;
            }

            // 使用日志系统
            Log.Info($"Applied effect: {effect.EffectType} ({value}) to {target.CreatureName}");
        }

        /// <summary>
        /// 移除单个效果
        /// </summary>
        private void RemoveEffect(BuffInstance buff, BuffEffect effect)
        {
            if (buff.Target == null) return;

            float value = effect.Value * buff.CurrentStacks;
            var target = buff.Target;

            switch (effect.EffectType)
            {
                case BuffEffectType.AttackBonus:
                    if (effect.IsPercentage)
                        target.Attack /= (1f + value * 0.01f);
                    else
                        target.Attack -= value;
                    break;

                case BuffEffectType.DefenseBonus:
                    if (effect.IsPercentage)
                        target.Defense /= (1f + value * 0.01f);
                    else
                        target.Defense -= value;
                    break;

                case BuffEffectType.SpeedBonus:
                    if (effect.IsPercentage)
                        target.Speed /= (1f + value * 0.01f);
                    else
                        target.Speed -= value;
                    break;

                // 恢复类和减免类效果在移除时不需要特殊处理
                case BuffEffectType.HealthRegeneration:
                case BuffEffectType.ManaRegeneration:
                case BuffEffectType.DamageReduction:
                case BuffEffectType.CriticalRateBonus:
                case BuffEffectType.StatusResistance:
                    break;
            }

            // 使用日志系统
            Log.Info($"Removed effect: {effect.EffectType} ({value}) from {target.CreatureName}");
        }

        /// <summary>
        /// 移除Buff实例
        /// </summary>
        private void RemoveBuffInstance(BuffInstance buff)
        {
            if (buff == null) return;

            // 移除效果
            RemoveBuffEffects(buff);

            _activeBuffs.Remove(buff);

            // 使用日志系统
            Log.Info($"Buff expired/removed: {buff.Data.BuffName} from {buff.Target.CreatureName}");
            EmitSignal(SignalName.BuffExpired, buff.Data.BuffId, buff.Target);
        }

        /// <summary>
        /// 清除所有Buff
        /// </summary>
        public void ClearAllBuffs()
        {
            foreach (var buff in _activeBuffs.ToList())
            {
                RemoveBuffInstance(buff);
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
                    _instance = new BuffManager
                    {
                        Name = "GlobalBuffManager"
                    };

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
            _ = Instance;
            // 使用日志系统
            Log.Info("Global BuffManager instance initialized");
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
                // 使用日志系统
                Log.Info("Global BuffManager instance cleaned up");
            }
        }
    }
}