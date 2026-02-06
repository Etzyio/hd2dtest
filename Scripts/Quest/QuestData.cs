using System.Collections.Generic;

namespace hd2dtest.Scripts.Quest
{
    /// <summary>
    /// 任务数据类
    /// </summary>
    public class QuestData
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 任务类型
        /// </summary>
        public QuestType Type { get; set; }
        
        /// <summary>
        /// 任务线ID
        /// </summary>
        public string QuestLineId { get; set; }
        
        /// <summary>
        /// 任务在线路中的顺序
        /// </summary>
        public int QuestLineOrder { get; set; }
        
        /// <summary>
        /// 任务依赖
        /// </summary>
        public List<QuestDependency> Dependencies { get; set; }
        
        /// <summary>
        /// 任务目标
        /// </summary>
        public List<QuestObjective> Objectives { get; set; }
        
        /// <summary>
        /// 任务奖励
        /// </summary>
        public List<QuestReward> Rewards { get; set; }
        
        /// <summary>
        /// 关联的成就
        /// </summary>
        public List<string> Achievements { get; set; }
        
        /// <summary>
        /// NPC关联
        /// </summary>
        public List<NPCAssociation> NPCAssociations { get; set; }
        
        /// <summary>
        /// 场景触发器
        /// </summary>
        public List<SceneTrigger> SceneTriggers { get; set; }
    }

    /// <summary>
    /// 任务类型枚举
    /// </summary>
    public enum QuestType
    {
        Main,  // 主线任务
        Side   // 支线任务
    }

    /// <summary>
    /// 任务依赖类
    /// </summary>
    public class QuestDependency
    {
        /// <summary>
        /// 依赖类型 (quest_completed, quest_in_progress, level)
        /// </summary>
        public string Type { get; set; }  // quest_completed, quest_in_progress, level
        
        /// <summary>
        /// 依赖的任务ID
        /// </summary>
        public string QuestId { get; set; }
        
        /// <summary>
        /// 依赖的等级
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// 操作符 (AND, OR)
        /// </summary>
        public string Operator { get; set; }  // AND, OR
    }

    /// <summary>
    /// 任务目标抽象类
    /// </summary>
    public abstract class QuestObjective
    {
        /// <summary>
        /// 目标ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 目标描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 检查目标是否完成
        /// </summary>
        /// <param name="progress">进度数据</param>
        /// <returns>是否完成</returns>
        public abstract bool IsCompleted(object progress);
    }

    /// <summary>
    /// 击杀目标
    /// </summary>
    public class KillObjective : QuestObjective
    {
        /// <summary>
        /// 敌人ID
        /// </summary>
        public string EnemyId { get; set; }
        
        /// <summary>
        /// 所需击杀数量
        /// </summary>
        public int RequiredCount { get; set; }
        
        /// <summary>
        /// 敌人生成位置
        /// </summary>
        public List<Vector3Data> SpawnLocations { get; set; }

        /// <summary>
        /// 检查击杀目标是否完成
        /// </summary>
        /// <param name="progress">进度数据（击杀数量）</param>
        /// <returns>是否完成</returns>
        public override bool IsCompleted(object progress)
        {
            if (progress is int count)
            {
                return count >= RequiredCount;
            }
            return false;
        }
    }

    /// <summary>
    /// 收集目标
    /// </summary>
    public class CollectObjective : QuestObjective
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; }
        
        /// <summary>
        /// 所需收集数量
        /// </summary>
        public int RequiredCount { get; set; }
        
        /// <summary>
        /// 完成时是否消耗物品
        /// </summary>
        public bool ConsumeOnComplete { get; set; }

        /// <summary>
        /// 检查收集目标是否完成
        /// </summary>
        /// <param name="progress">进度数据（收集数量）</param>
        /// <returns>是否完成</returns>
        public override bool IsCompleted(object progress)
        {
            if (progress is int count)
            {
                return count >= RequiredCount;
            }
            return false;
        }
    }

    /// <summary>
    /// 对话目标
    /// </summary>
    public class TalkObjective : QuestObjective
    {
        /// <summary>
        /// NPC ID
        /// </summary>
        public string NPCId { get; set; }

        /// <summary>
        /// 检查对话目标是否完成
        /// </summary>
        /// <param name="progress">进度数据（是否完成）</param>
        /// <returns>是否完成</returns>
        public override bool IsCompleted(object progress)
        {
            return progress is bool && (bool)progress;
        }
    }

    /// <summary>
    /// 到达位置目标
    /// </summary>
    public class ReachLocationObjective : QuestObjective
    {
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3Data Location { get; set; }
        
        /// <summary>
        /// 判定半径
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 检查到达位置目标是否完成
        /// </summary>
        /// <param name="progress">进度数据（是否完成）</param>
        /// <returns>是否完成</returns>
        public override bool IsCompleted(object progress)
        {
            return progress is bool && (bool)progress;
        }
    }

    /// <summary>
    /// 任务奖励抽象类
    /// </summary>
    public abstract class QuestReward
    {
        /// <summary>
        /// 发放奖励
        /// </summary>
        public abstract void Grant();
    }

    /// <summary>
    /// 经验值奖励
    /// </summary>
    public class ExperienceReward : QuestReward
    {
        /// <summary>
        /// 经验值数量
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 发放经验值奖励
        /// </summary>
        public override void Grant()
        {
            // 给予经验值
            QuestManager.Instance.GrantExperience(Amount);
        }
    }

    /// <summary>
    /// 物品奖励
    /// </summary>
    public class ItemReward : QuestReward
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; }
        
        /// <summary>
        /// 物品数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 发放物品奖励
        /// </summary>
        public override void Grant()
        {
            // 给予物品
            QuestManager.Instance.GrantItem(ItemId, Count);
        }
    }

    /// <summary>
    /// 货币奖励
    /// </summary>
    public class CurrencyReward : QuestReward
    {
        /// <summary>
        /// 货币类型
        /// </summary>
        public string CurrencyType { get; set; }
        
        /// <summary>
        /// 货币数量
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 发放货币奖励
        /// </summary>
        public override void Grant()
        {
            // 给予货币
            QuestManager.Instance.GrantCurrency(CurrencyType, Amount);
        }
    }

    /// <summary>
    /// 装备奖励
    /// </summary>
    public class EquipmentReward : QuestReward
    {
        /// <summary>
        /// 装备ID
        /// </summary>
        public string EquipmentId { get; set; }

        /// <summary>
        /// 发放装备奖励
        /// </summary>
        public override void Grant()
        {
            // 给予装备
            QuestManager.Instance.GrantEquipment(EquipmentId);
        }
    }

    /// <summary>
    /// NPC关联
    /// </summary>
    public class NPCAssociation
    {
        /// <summary>
        /// NPC ID
        /// </summary>
        public string NPCId { get; set; }
        
        /// <summary>
        /// 对话类型
        /// </summary>
        public List<NPCTalkType> TalkTypes { get; set; }
    }

    /// <summary>
    /// NPC对话类型枚举
    /// </summary>
    public enum NPCTalkType
    {
        Accept,   // 接取任务
        HandIn,   // 交付任务
        Progress, // 任务进度对话
        Complete  // 任务完成对话
    }

    /// <summary>
    /// 场景触发器
    /// </summary>
    public class SceneTrigger
    {
        /// <summary>
        /// 场景ID
        /// </summary>
        public string SceneId { get; set; }
        
        /// <summary>
        /// 触发类型
        /// </summary>
        public TriggerType TriggerType { get; set; }
        
        /// <summary>
        /// 动画名称
        /// </summary>
        public string AnimationName { get; set; }
        
        /// <summary>
        /// 是否可跳过
        /// </summary>
        public bool Skipable { get; set; }
    }

    /// <summary>
    /// 触发器类型枚举
    /// </summary>
    public enum TriggerType
    {
        OnStart,    // 任务开始时
        OnComplete, // 任务完成时
        OnObjective // 完成特定目标时
    }

    /// <summary>
    /// 三维向量数据类
    /// </summary>
    public class Vector3Data
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public float X { get; set; }
        
        /// <summary>
        /// Y坐标
        /// </summary>
        public float Y { get; set; }
        
        /// <summary>
        /// Z坐标
        /// </summary>
        public float Z { get; set; }
    }
}
