using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Modules; // 用于 Creature 引用

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    /// <summary>
    /// 伤害技能事件，用于在技能执行时造成伤害
    /// </summary>
    [GlobalClass]
    public partial class DamageSkillEvent : SkillEvent
    {
        /// <summary>
        /// 伤害倍率，用于计算最终伤害值
        /// </summary>
        /// <value>伤害倍率，默认为 1.0</value>
        public float DamageMultiplier { get; set; } = 1.0f;

        /// <summary>
        /// 伤害类型，用于弱点判定
        /// </summary>
        /// <value>伤害类型字符串，如"Physical"、"Fire"、"Ice"等，默认为"Physical"</value>
        public string DamageType { get; set; } = "Physical";

        /// <summary>
        /// 执行伤害技能事件
        /// </summary>
        /// <param name="context">技能执行上下文，包含施法者和目标信息</param>
        /// <remarks>
        /// 该方法根据施法者的攻击力和伤害倍率计算基础伤害，
        /// 然后使用 DamageCalculator 计算考虑防御和弱点后的实际伤害，
        /// 最后对目标造成伤害。
        /// </remarks>
        public override void Execute(SkillExecutionContext context)
        {
            if (context.Target != null && context.Caster != null)
            {
                float baseDamage = context.Caster.Attack * DamageMultiplier;
                Log.Info($"[SkillEvent] Dealing {baseDamage} {DamageType} damage to {context.Target.CreatureName}");

                // 集成实际伤害计算器
                var damageResult = DamageCalculator.CalculateSkillDamage(context.Caster, context.Target, baseDamage, DamageType);
                context.Target.TakeDamage(damageResult, DamageType);
            }
        }
    }

    /// <summary>
    /// 治疗技能事件，用于在技能执行时恢复生命值
    /// </summary>
    [GlobalClass]
    public partial class HealSkillEvent : SkillEvent
    {
        /// <summary>
        /// 治疗倍率，用于计算治疗量
        /// </summary>
        /// <value>治疗倍率，默认为 1.0</value>
        public float HealMultiplier { get; set; } = 1.0f;

        /// <summary>
        /// 基础治疗量，固定增加的治疗值
        /// </summary>
        /// <value>基础治疗量，默认为 0</value>
        public float BaseHeal { get; set; } = 0f;

        /// <summary>
        /// 执行治疗技能事件
        /// </summary>
        /// <param name="context">技能执行上下文，包含施法者和目标信息</param>
        /// <remarks>
        /// 该方法根据基础治疗量和施法者的攻击力计算治疗量，
        /// 计算公式：BaseHeal + (Caster.Attack * 0.5 * HealMultiplier)
        /// 治疗量会添加到目标的生命值，但不超过最大生命值。
        /// </remarks>
        public override void Execute(SkillExecutionContext context)
        {
            if (context.Target != null && context.Caster != null)
            {
                // 简单治疗公式
                float healAmount = BaseHeal + (context.Caster.Attack * 0.5f * HealMultiplier); // 暂时使用攻击力作为魔法强度的代理
                Log.Info($"[SkillEvent] Healing {healAmount} to {context.Target.CreatureName}");

                context.Target.Health += healAmount;
                if (context.Target.Health > context.Target.MaxHealth)
                    context.Target.Health = context.Target.MaxHealth;
            }
        }
    }

    /// <summary>
    /// Buff 技能事件，用于在技能执行时施加 Buff 效果
    /// </summary>
    [GlobalClass]
    public partial class BuffSkillEvent : SkillEvent
    {
        /// <summary>
        /// Buff 的唯一标识符
        /// </summary>
        /// <value>Buff ID 字符串</value>
        public string BuffId { get; set; }

        /// <summary>
        /// Buff 持续时间（秒）
        /// </summary>
        /// <value>持续时间，默认为 3.0 秒</value>
        public float Duration { get; set; } = 3.0f;

        /// <summary>
        /// Buff 效果值，例如增加的攻击力数值
        /// </summary>
        /// <value>效果值，默认为 0</value>
        public float Value { get; set; } = 0f; // 通用值（例如，+10 攻击力）

        /// <summary>
        /// 执行 Buff 技能事件
        /// </summary>
        /// <param name="context">技能执行上下文，包含施法者和目标信息</param>
        /// <remarks>
        /// 该方法调用 BuffManager 对目标施加指定的 Buff 效果。
        /// Buff 的具体效果由 BuffId 对应的 Buff 数据定义。
        /// </remarks>
        public override void Execute(SkillExecutionContext context)
        {
            if (context.Target != null)
            {
                Log.Info($"[SkillEvent] Applying Buff {BuffId} (Value: {Value}) for {Duration}s to {context.Target.CreatureName}");
                // 集成 Buff 系统
                BuffManagerInstance.Instance?.ApplyBuff(BuffId, context.Caster, context.Target);
            }
        }
    }

    /// <summary>
    /// 生成特效事件，用于在技能执行时生成视觉效果
    /// </summary>
    [GlobalClass]
    public partial class SpawnEffectEvent : SkillEvent
    {
        /// <summary>
        /// 特效场景资源
        /// </summary>
        /// <value>PackedScene 类型的特效场景</value>
        public PackedScene EffectScene { get; set; }

        /// <summary>
        /// 是否将特效附加到目标节点
        /// </summary>
        /// <value>true 表示附加到目标，false 表示生成在世界坐标，默认为 false</value>
        public bool AttachToTarget { get; set; } = false;

        /// <summary>
        /// 特效相对于生成位置的偏移量
        /// </summary>
        /// <value>Vector3 类型的偏移量，默认为 Vector3.Zero</value>
        public Vector3 Offset { get; set; } = Vector3.Zero;

        /// <summary>
        /// 特效持续时间（秒）
        /// </summary>
        /// <value>持续时间，默认为 2.0 秒</value>
        public float Duration { get; set; } = 2.0f;

        /// <summary>
        /// 执行生成特效事件
        /// </summary>
        /// <param name="context">技能执行上下文，包含施法者和目标信息</param>
        /// <remarks>
        /// 该方法实例化特效场景并设置其位置：
        /// - 如果 AttachToTarget 为 true，特效将作为目标的子节点
        /// - 否则，特效将生成在目标的世界坐标位置
        /// 特效会在指定持续时间后自动销毁。
        /// </remarks>
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
                // 根据上下文确定位置（目标位置或特定位置）
                if (context.TargetNode != null)
                    effectInstance.GlobalPosition = context.TargetNode.GlobalPosition + Offset;
                else
                    effectInstance.GlobalPosition = new Vector3(context.TargetPosition.X, 0, context.TargetPosition.Y) + Offset;
            }

            Log.Info($"[SkillEvent] Spawning effect {EffectScene.ResourceName}");

            // 如果脚本未处理，自动销毁逻辑
            SceneTreeTimer timer = context.CasterNode.GetTree().CreateTimer(Duration);
            timer.Timeout += () => { if (Godot.GodotObject.IsInstanceValid(effectInstance)) effectInstance.QueueFree(); };
        }
    }

    /// <summary>
    /// 播放声音事件，用于在技能执行时播放音效
    /// </summary>
    [GlobalClass]
    public partial class PlaySoundEvent : SkillEvent
    {
        /// <summary>
        /// 要播放的音频流资源
        /// </summary>
        /// <value>AudioStream 类型的音频资源</value>
        public AudioStream Sound { get; set; }

        /// <summary>
        /// 音量（分贝）
        /// </summary>
        /// <value>音量值，默认为 0.0dB</value>
        public float VolumeDb { get; set; } = 0.0f;

        /// <summary>
        /// 执行播放声音事件
        /// </summary>
        /// <param name="context">技能执行上下文</param>
        /// <remarks>
        /// 该方法记录播放声音的日志。
        /// 在实际游戏中，应集成 AudioSystem 来播放声音。
        /// </remarks>
        public override void Execute(SkillExecutionContext context)
        {
            Log.Info($"[SkillEvent] Playing sound: {Sound?.ResourceName}");
            // AudioSystem.Play(Sound); // 播放声音
        }
    }
}
