using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.Battle
{
    [GlobalClass]
    public partial class PassiveSkill : Resource
    {
        [Export] public string PassiveName { get; set; } = "New Passive";
        [Export] public string Description { get; set; } = "";
        
        // Bonus Stats (additive or multiplicative)
        [Export] public float HealthBonus { get; set; } = 0f;
        [Export] public float AttackBonus { get; set; } = 0f;
        [Export] public float DefenseBonus { get; set; } = 0f;
        [Export] public float SpeedBonus { get; set; } = 0f;
        [Export] public float CritRateBonus { get; set; } = 0f;

        // Multipliers (e.g., 1.1 for +10%)
        [Export] public float HealthMultiplier { get; set; } = 1f;
        [Export] public float AttackMultiplier { get; set; } = 1f;
        [Export] public float DefenseMultiplier { get; set; } = 1f;

        // Custom effects could be handled by ID or a more complex system
        [Export] public string EffectId { get; set; } = ""; 
    }
}
