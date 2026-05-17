/*
 * File: Profession.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 职业系统相关类，定义游戏中的职业、被动技能和职业注册机制。
 * 支持主职业、副职业系统，被动技能分类和冲突检测。
 * 
 * Key Features:
 * - 职业定义：包含职业ID、名称、描述和被动技能列表
 * - 被动技能分类：主职业专属、副职业专属、通用
 * - 被动效果系统：属性加成、堆叠组管理
 * - 职业注册：集中管理所有职业和被动技能
 * - 冲突检测：被动技能互斥检查
 * - 完整的异常处理和日志记录
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 被动技能分类枚举
    /// </summary>
    /// <remarks>
    /// 定义被动技能的适用范围：主职业专属、副职业专属、通用
    /// </remarks>
    public enum PassiveCategory
    {
        MainExclusive,
        SubExclusive,
        General
    }

    /// <summary>
    /// 被动技能效果定义
    /// </summary>
    /// <remarks>
    /// 定义被动技能对角色属性的具体影响，支持堆叠组管理
    /// </remarks>
    public class PassiveEffect
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        /// <value>要影响的属性名称，如：Attack、Defense、Health、Speed等</value>
        public string Stat = "Attack";

        /// <summary>
        /// 效果值
        /// </summary>
        /// <value>属性加成的值，可为正数（增强）或负数（削弱）</value>
        public float Value = 0f;

        /// <summary>
        /// 堆叠组
        /// </summary>
        /// <value>用于分组管理可堆叠或互斥的被动效果</value>
        public string StackGroup = "";
    }

    /// <summary>
    /// 被动技能定义
    /// </summary>
    /// <remarks>
    /// 定义被动技能的完整信息，包括属性、效果和冲突关系
    /// </remarks>
    public class PassiveSkillDef
    {
        /// <summary>
        /// 被动技能ID
        /// </summary>
        /// <value>被动技能的唯一标识符</value>
        public string Id = "";

        /// <summary>
        /// 被动技能名称
        /// </summary>
        /// <value>被动技能的显示名称</value>
        public string Name = "";

        /// <summary>
        /// 被动技能描述
        /// </summary>
        /// <value>被动技能的详细描述信息</value>
        public string Description = "";

        /// <summary>
        /// 被动技能分类
        /// </summary>
        /// <value>被动技能的适用范围分类</value>
        public PassiveCategory Category = PassiveCategory.General;

        /// <summary>
        /// 优先级
        /// </summary>
        /// <value>被动技能的优先级，用于冲突解决</value>
        public int Priority = 0;

        /// <summary>
        /// 冲突技能列表
        /// </summary>
        /// <value>与此被动技能互斥的其他被动技能ID列表</value>
        public List<string> ConflictsWith { get; set; } = [];

        /// <summary>
        /// 效果列表
        /// </summary>
        /// <value>被动技能产生的所有属性效果列表</value>
        public List<PassiveEffect> Effects { get; set; } = [];
    }

    /// <summary>
    /// 职业定义
    /// </summary>
    /// <remarks>
    /// 定义游戏中的职业，包含职业名称、描述和专属被动技能
    /// </remarks>
    public class Profession
    {
        /// <summary>
        /// 职业ID
        /// </summary>
        /// <value>职业的唯一标识符</value>
        public string Id = "";

        /// <summary>
        /// 职业名称
        /// </summary>
        /// <value>职业的显示名称</value>
        public string Name = "";

        /// <summary>
        /// 职业描述
        /// </summary>
        /// <value>职业的详细描述信息</value>
        public string Description = "";

        /// <summary>
        /// 被动技能列表
        /// </summary>
        /// <value>该职业拥有的所有被动技能定义列表</value>
        public List<PassiveSkillDef> Passives { get; set; } = [];
    }

    /// <summary>
    /// 被动技能拥有权
    /// </summary>
    /// <remarks>
    /// 记录玩家对某个被动技能的拥有状态，包括来源职业和精通状态
    /// </remarks>
    public class PassiveOwnership
    {
        /// <summary>
        /// 被动技能ID
        /// </summary>
        /// <value>被动技能的唯一标识符</value>
        public string PassiveId = "";

        /// <summary>
        /// 来源职业ID
        /// </summary>
        /// <value>获得该被动技能的职业ID</value>
        public string SourceProfessionId = "";

        /// <summary>
        /// 是否精通
        /// </summary>
        /// <value>true 表示该被动技能已精通，可能有额外效果</value>
        public bool Mastered = false;
    }

    /// <summary>
    /// 职业注册管理器
    /// </summary>
    /// <remarks>
    /// 静态类，负责集中管理所有职业和被动技能的注册与查询
    /// </remarks>
    public static class ProfessionRegistry
    {
        /// <summary>
        /// 职业注册表
        /// </summary>
        private static readonly Dictionary<string, Profession> _professions = new();

        /// <summary>
        /// 被动技能注册表
        /// </summary>
        private static readonly Dictionary<string, PassiveSkillDef> _passives = new();

        /// <summary>
        /// 注册职业
        /// </summary>
        /// <param name="p">要注册的职业对象</param>
        /// <remarks>
        /// 将职业及其所有被动技能注册到全局注册表中
        /// 如果职业为null或ID为空，则不进行注册
        /// </remarks>
        public static void RegisterProfession(Profession p)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.Id)) return;
            _professions[p.Id] = p;
            foreach (var ps in p.Passives)
            {
                _passives[ps.Id] = ps;
            }
        }

        /// <summary>
        /// 获取职业
        /// </summary>
        /// <param name="id">职业ID</param>
        /// <returns>对应的职业对象，如果不存在则返回null</returns>
        public static Profession GetProfession(string id) =>
            id != null && _professions.TryGetValue(id, out var v) ? v : null;

        /// <summary>
        /// 获取被动技能定义
        /// </summary>
        /// <param name="id">被动技能ID</param>
        /// <returns>对应的被动技能定义，如果不存在则返回null</returns>
        public static PassiveSkillDef GetPassive(string id) =>
            id != null && _passives.TryGetValue(id, out var v) ? v : null;
    }
}
