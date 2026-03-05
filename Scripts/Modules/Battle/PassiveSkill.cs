using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.Battle
{
    /// <summary>
    /// 被动技能类，定义角色的被动增益效果
    /// </summary>
    [GlobalClass]
    public partial class PassiveSkill : Resource
    {
        /// <summary>
        /// 被动技能名称
        /// </summary>
        /// <value>技能的名称字符串，默认为"New Passive"</value>
        public string PassiveName { get; set; } = "New Passive";

        /// <summary>
        /// 被动技能描述
        /// </summary>
        /// <value>技能的详细描述文本</value>
        public string Description { get; set; } = "";

        /// <summary>
        /// 生命值加成（加法）
        /// </summary>
        /// <value>增加的生命值数值，默认为 0</value>
        public float HealthBonus { get; set; } = 0f;

        /// <summary>
        /// 攻击力加成（加法）
        /// </summary>
        /// <value>增加的攻击力数值，默认为 0</value>
        public float AttackBonus { get; set; } = 0f;

        /// <summary>
        /// 防御力加成（加法）
        /// </summary>
        /// <value>增加的防御力数值，默认为 0</value>
        public float DefenseBonus { get; set; } = 0f;

        /// <summary>
        /// 速度加成（加法）
        /// </summary>
        /// <value>增加的速度数值，默认为 0</value>
        public float SpeedBonus { get; set; } = 0f;

        /// <summary>
        /// 暴击率加成（加法）
        /// </summary>
        /// <value>增加的暴击率数值，默认为 0</value>
        public float CritRateBonus { get; set; } = 0f;

        /// <summary>
        /// 生命值乘数（乘法）
        /// </summary>
        /// <value>生命值倍率，默认为 1.0（例如 1.1 表示 +10%）</value>
        public float HealthMultiplier { get; set; } = 1f;

        /// <summary>
        /// 攻击力乘数（乘法）
        /// </summary>
        /// <value>攻击力倍率，默认为 1.0（例如 1.1 表示 +10%）</value>
        public float AttackMultiplier { get; set; } = 1f;

        /// <summary>
        /// 防御力乘数（乘法）
        /// </summary>
        /// <value>防御力倍率，默认为 1.0（例如 1.1 表示 +10%）</value>
        public float DefenseMultiplier { get; set; } = 1f;

        /// <summary>
        /// 自定义效果 ID
        /// </summary>
        /// <value>效果标识符字符串，用于实现特殊效果</value>
        /// <remarks>
        /// 自定义效果可以通过此 ID 或更复杂的系统处理。
        /// 可以在效果管理器中根据此 ID 查找并应用相应的特殊效果。
        /// </remarks>
        public string EffectId { get; set; } = "";
    }
}
