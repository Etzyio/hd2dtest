using Godot;
using Godot.Collections;

namespace hd2dtest.Scripts.Modules.Battle
{
    [GlobalClass]
    public partial class BattleClass : Resource
    {
        [Export] public string ClassName { get; set; } = "New Class";
        [Export] public string Description { get; set; } = "";
        
        // Base stats growth or multipliers
        [Export] public float HealthGrowth { get; set; } = 1.0f;
        [Export] public float AttackGrowth { get; set; } = 1.0f;
        [Export] public float DefenseGrowth { get; set; } = 1.0f;
        [Export] public float SpeedGrowth { get; set; } = 1.0f;
        [Export] public float ManaGrowth { get; set; } = 1.0f;

        // Skills available at specific levels
        // Key: Level, Value: Skill Resource or ID
        // Since Dictionary export is tricky in Godot C#, we might use arrays
        [Export] public Array<SkillEntry> SkillsToLearn { get; set; } = new Array<SkillEntry>();

        // Passive Skills available to learn
        [Export] public Array<PassiveSkillEntry> PassivesToLearn { get; set; } = new Array<PassiveSkillEntry>();
    }

    [GlobalClass]
    public partial class SkillEntry : Resource
    {
        [Export] public int LevelRequired { get; set; } = 1;
        [Export] public Skill SkillResource { get; set; } // Assuming Skill will be a Resource
    }

    [GlobalClass]
    public partial class PassiveSkillEntry : Resource
    {
        [Export] public int LevelRequired { get; set; } = 1;
        [Export] public PassiveSkill PassiveResource { get; set; }
    }
}
