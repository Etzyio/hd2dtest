using Godot;
using System.Collections.Generic;
using Godot.Collections;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    [GlobalClass]
    public partial class SkillData : Resource
    {
        [ExportCategory("Basic Info")]
        [Export] public string SkillId { get; set; } = System.Guid.NewGuid().ToString();
        [Export] public string SkillName { get; set; } = "New Skill";
        [Export] public string Description { get; set; } = "";
        [Export] public Texture2D Icon { get; set; }

        [ExportCategory("Properties")]
        [Export] public float Cooldown { get; set; } = 1.0f;
        [Export] public int ManaCost { get; set; } = 10;
        [Export] public float Range { get; set; } = 5.0f;
        [Export] public bool IsTargeted { get; set; } = true;

        [ExportCategory("Execution Timeline")]
        [Export] public Array<SkillPhase> Phases { get; set; } = new Array<SkillPhase>();
    }

    [GlobalClass]
    public partial class SkillPhase : Resource
    {
        [Export] public string PhaseName { get; set; } = "Cast";
        [Export] public float Duration { get; set; } = 0.5f;
        [Export] public Array<SkillEvent> Events { get; set; } = new Array<SkillEvent>();
    }

    [GlobalClass]
    public abstract partial class SkillEvent : Resource
    {
        [Export] public float NormalizedTime { get; set; } = 0.0f; // 0.0 to 1.0 within phase

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
