using Godot;
using Godot.Collections;

namespace hd2dtest.Scripts.Modules.Battle
{
    /// <summary>
    /// 战斗职业类，定义角色的职业属性和成长曲线
    /// </summary>
    [GlobalClass]
    public partial class BattleClass : Resource
    {
        /// <summary>
        /// 职业名称
        /// </summary>
        /// <value>职业的名称字符串，默认为"New Class"</value>
         public string ClassName { get; set; } = "New Class";
        
        /// <summary>
        /// 职业描述
        /// </summary>
        /// <value>职业的详细描述文本</value>
         public string Description { get; set; } = "";
        
        /// <summary>
        /// 生命值成长系数
        /// </summary>
        /// <value>成长系数，默认为 1.0</value>
        /// <remarks>用于计算升级时的生命值增加量</remarks>
         public float HealthGrowth { get; set; } = 1.0f;
        
        /// <summary>
        /// 攻击力成长系数
        /// </summary>
        /// <value>成长系数，默认为 1.0</value>
        /// <remarks>用于计算升级时的攻击力增加量</remarks>
         public float AttackGrowth { get; set; } = 1.0f;
        
        /// <summary>
        /// 防御力成长系数
        /// </summary>
        /// <value>成长系数，默认为 1.0</value>
        /// <remarks>用于计算升级时的防御力增加量</remarks>
         public float DefenseGrowth { get; set; } = 1.0f;
        
        /// <summary>
        /// 速度成长系数
        /// </summary>
        /// <value>成长系数，默认为 1.0</value>
        /// <remarks>用于计算升级时的速度增加量</remarks>
         public float SpeedGrowth { get; set; } = 1.0f;
        
        /// <summary>
        /// 魔法值成长系数
        /// </summary>
        /// <value>成长系数，默认为 1.0</value>
        /// <remarks>用于计算升级时的魔法值增加量</remarks>
         public float ManaGrowth { get; set; } = 1.0f;

        /// <summary>
        /// 可学习技能列表
        /// </summary>
        /// <value>包含等级和对应技能资源的数组</value>
        /// <remarks>
        /// 由于在 Godot C# 中导出 Dictionary 很困难，使用数组代替。
        /// 每个 SkillEntry 包含学习该技能所需的等级和技能资源。
        /// </remarks>
         public Array<SkillEntry> SkillsToLearn { get; set; } = new Array<SkillEntry>();

        /// <summary>
        /// 可学习被动技能列表
        /// </summary>
        /// <value>包含等级和对应被动技能资源的数组</value>
        /// <remarks>
        /// 每个 PassiveSkillEntry 包含学习该被动技能所需的等级和被动技能资源。
        /// </remarks>
         public Array<PassiveSkillEntry> PassivesToLearn { get; set; } = new Array<PassiveSkillEntry>();
    }

    /// <summary>
    /// 技能条目类，用于定义职业在特定等级可学习的技能
    /// </summary>
    [GlobalClass]
    public partial class SkillEntry : Resource
    {
        /// <summary>
        /// 学习该技能所需的等级
        /// </summary>
        /// <value>等级要求，默认为 1</value>
         public int LevelRequired { get; set; } = 1;
        
        /// <summary>
        /// 技能资源引用
        /// </summary>
        /// <value>Skill 类型的技能资源</value>
         public Skill SkillResource { get; set; } // Assuming Skill will be a Resource
    }

    /// <summary>
    /// 被动技能条目类，用于定义职业在特定等级可学习的被动技能
    /// </summary>
    [GlobalClass]
    public partial class PassiveSkillEntry : Resource
    {
        /// <summary>
        /// 学习该被动技能所需的等级
        /// </summary>
        /// <value>等级要求，默认为 1</value>
         public int LevelRequired { get; set; } = 1;
        
        /// <summary>
        /// 被动技能资源引用
        /// </summary>
        /// <value>PassiveSkill 类型的被动技能资源</value>
         public PassiveSkill PassiveResource { get; set; }
    }
}
