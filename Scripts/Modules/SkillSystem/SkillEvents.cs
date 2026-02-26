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
                float baseDamage = context.Caster.Attack * DamageMultiplier;
                Log.Info($"[SkillEvent] Dealing {baseDamage} {DamageType} damage to {context.Target.CreatureName}");
                
                // TODO: Integrate with actual DamageCalculator
                // context.Target.TakeDamage(baseDamage, DamageType); 
            }
        }
    }

    [GlobalClass]
    public partial class HealSkillEvent : SkillEvent
    {
        [Export] public float HealMultiplier { get; set; } = 1.0f;
        [Export] public float BaseHeal { get; set; } = 0f;

        public override void Execute(SkillExecutionContext context)
        {
            if (context.Target != null && context.Caster != null)
            {
                // Simple heal formula
                float healAmount = BaseHeal + (context.Caster.Attack * 0.5f * HealMultiplier); // Using Attack as Magic Power proxy for now
                Log.Info($"[SkillEvent] Healing {healAmount} to {context.Target.CreatureName}");
                
                context.Target.Health += healAmount;
                if (context.Target.Health > context.Target.MaxHealth)
                    context.Target.Health = context.Target.MaxHealth;
            }
        }
    }

    [GlobalClass]
    public partial class BuffSkillEvent : SkillEvent
    {
        [Export] public string BuffId { get; set; }
        [Export] public float Duration { get; set; } = 3.0f;
        [Export] public float Value { get; set; } = 0f; // Generic value (e.g., +10 Attack)

        public override void Execute(SkillExecutionContext context)
        {
            if (context.Target != null)
            {
                Log.Info($"[SkillEvent] Applying Buff {BuffId} (Value: {Value}) for {Duration}s to {context.Target.CreatureName}");
                // TODO: Integrate with BuffSystem
                // context.Target.AddBuff(BuffId, Duration, Value);
            }
        }
    }

    [GlobalClass]
    public partial class SpawnEffectEvent : SkillEvent
    {
        [Export] public PackedScene EffectScene { get; set; }
        [Export] public bool AttachToTarget { get; set; } = false;
        [Export] public Vector3 Offset { get; set; } = Vector3.Zero;
        [Export] public float Duration { get; set; } = 2.0f;

        public override void Execute(SkillExecutionContext context)
        {
            if (EffectScene == null || context.CasterNode == null) return;

            Node root = context.CasterNode.GetTree().Root;
            Node3D effectInstance = EffectScene.Instantiate<Node3D>();
            
            if (effectInstance == null)
            {
                Log.Error("SpawnEffectEvent: EffectScene is not a Node3D");
                return;
            }

            if (AttachToTarget && context.TargetNode != null)
            {
                context.TargetNode.AddChild(effectInstance);
                effectInstance.Position = Offset;
            }
            else
            {
                root.AddChild(effectInstance);
                // Determine position based on context (Target pos or specific pos)
                if (context.TargetNode != null)
                    effectInstance.GlobalPosition = context.TargetNode.GlobalPosition + Offset;
                else
                    effectInstance.GlobalPosition = new Vector3(context.TargetPosition.X, 0, context.TargetPosition.Y) + Offset;
            }

            Log.Info($"[SkillEvent] Spawning effect {EffectScene.ResourceName}");
            
            // Auto-destroy logic if script not handling it
            SceneTreeTimer timer = context.CasterNode.GetTree().CreateTimer(Duration);
            timer.Timeout += () => { if (Godot.GodotObject.IsInstanceValid(effectInstance)) effectInstance.QueueFree(); };
        }
    }

    [GlobalClass]
    public partial class PlaySoundEvent : SkillEvent
    {
        [Export] public AudioStream Sound { get; set; }
        [Export] public float VolumeDb { get; set; } = 0.0f;

        public override void Execute(SkillExecutionContext context)
        {
            Log.Info($"[SkillEvent] Playing sound: {Sound?.ResourceName}");
            // AudioSystem.Play(Sound);
        }
    }
}
