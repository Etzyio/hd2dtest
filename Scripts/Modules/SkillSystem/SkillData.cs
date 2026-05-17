/*
 * File: SkillData.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 技能数据模型类，定义技能的基本信息和执行时间线。
 * 包含技能数据、阶段数据、技能事件基类和执行上下文。
 * 
 * Key Features:
 * - SkillData：技能基本数据（ID、名称、描述、冷却时间、魔法消耗等）
 * - SkillPhase：技能阶段（阶段名称、持续时间、事件列表）
 * - SkillEvent：技能事件基类（标准化时间、执行方法）
 * - SkillExecutionContext：技能执行上下文（施法者、目标、节点引用等）
 */

using Godot;
using System.Collections.Generic;
using Godot.Collections;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    /// <summary>
    /// 技能数据类，定义技能的基本信息和执行时间线
    /// </summary>
    [GlobalClass]
    public partial class SkillData : Resource
    {
        [ExportCategory("Basic Info")]
        public string SkillId { get; set; } = System.Guid.NewGuid().ToString();
        public string SkillName { get; set; } = "New Skill";
        public string Description { get; set; } = "";
        public Texture2D Icon { get; set; }

        [ExportCategory("Properties")]
        public float Cooldown { get; set; } = 1.0f;
        public int ManaCost { get; set; } = 10;
        public float Range { get; set; } = 5.0f;
        public bool IsTargeted { get; set; } = true;

        [ExportCategory("Execution Timeline")]
        public Array<SkillPhase> Phases { get; set; } = new Array<SkillPhase>();
    }

    [GlobalClass]
    public partial class SkillPhase : Resource
    {
        public string PhaseName { get; set; } = "Cast";
        public float Duration { get; set; } = 0.5f;
        public Array<SkillEvent> Events { get; set; } = new Array<SkillEvent>();
    }

    [GlobalClass]
    public abstract partial class SkillEvent : Resource
    {
        public float NormalizedTime { get; set; } = 0.0f; // 0.0 to 1.0 within phase

        public virtual void Execute(SkillExecutionContext context)
        {
            // 在子类中重写
        }
    }

    // 在执行期间传递给事件的上下文
    public class SkillExecutionContext
    {
        public Creature Caster { get; set; }
        public Creature Target { get; set; }
        public Node3D CasterNode { get; set; } // 视觉节点
        public Node3D TargetNode { get; set; } // 视觉节点
        public Vector2 TargetPosition { get; set; }
        public SkillData Skill { get; set; }
    }
}
