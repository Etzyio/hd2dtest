/*
 * File: BuffEffect.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose: Buff 效果与数据模型定义。
 */

using Godot;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    [GlobalClass]
    public partial class BuffEffect : Resource
    {
        public BuffEffectType EffectType { get; set; } = BuffEffectType.AttackBonus;
        public float Value { get; set; } = 0f;
        public bool IsPercentage { get; set; } = false;
        public string Description { get; set; } = "";
    }

    [GlobalClass]
    public partial class BuffData : Resource
    {
        public string BuffId { get; set; } = System.Guid.NewGuid().ToString();
        public string BuffName { get; set; } = TranslationServer.Translate("buff_default_name");
        public string Description { get; set; } = "";
        public Texture2D Icon { get; set; }
        public Godot.Collections.Array<BuffEffect> Effects { get; set; } = [];
        public BuffDurationType DurationType { get; set; } = BuffDurationType.Duration;
        public float Duration { get; set; } = 10f;
        public int TurnCount { get; set; } = 3;
        public int MaxStacks { get; set; } = 1;
        public bool IsPositive { get; set; } = true;
        public bool CanBeDispelled { get; set; } = true;
        public bool RefreshDurationOnReapply { get; set; } = true;
    }
}
