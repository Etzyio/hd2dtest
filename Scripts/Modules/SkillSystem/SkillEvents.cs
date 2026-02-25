using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Modules; // For Creature

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    [GlobalClass]
    public partial class DamageSkillEvent : SkillEvent
    {
        [Export] public float DamageMultiplier { get; set; } = 1.0f;
        [Export] public string DamageType { get; set; } = "Physical";
        
        public override void Execute(SkillExecutionContext context)
        {
            if (context.Target != null && context.Caster != null)
            {
                // In a real system, we'd use a DamageCalculator here
                // For now, let's just log and simulate
                float baseDamage = context.Caster.Attack * DamageMultiplier;
                
                // Assuming Creature has TakeDamage method that accepts damage amount and type
                // But existing Creature.TakeDamage takes (Creature source, Skill skill)
                // We might need to adapt or use a temporary Skill object wrapper
                
                Log.Info($"[SkillEvent] Dealing {baseDamage} {DamageType} damage to {context.Target.CreatureName}");
                
                // Construct a temporary skill for the old system compatibility if needed
                // or extend Creature to accept raw damage values
                
                // For prototype:
                // context.Target.TakeDamage(baseDamage, DamageType); 
            }
        }
    }

    [GlobalClass]
    public partial class PlaySoundEvent : SkillEvent
    {
        [Export] public AudioStream Sound { get; set; }
        [Export] public float VolumeDb { get; set; } = 0.0f;

        public override void Execute(SkillExecutionContext context)
        {
            // Play sound at Caster position
            Log.Info($"[SkillEvent] Playing sound: {Sound?.ResourceName}");
            // AudioSystem.Play(Sound, context.Caster.GlobalPosition);
        }
    }
}
