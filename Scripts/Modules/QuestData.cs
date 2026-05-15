using System.Collections.Generic;
using Godot;
using hd2dtest.Scripts.Quest;

namespace hd2dtest.Scripts.Modules
{
    public class QuestData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public QuestType Type { get; set; }
        public string QuestLineId { get; set; }
        public int Order { get; set; }
        public List<string> Requires { get; set; } = new List<string>();
        public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
        public List<QuestReward> Rewards { get; set; } = new List<QuestReward>();
        public List<string> Achievements { get; set; } = new List<string>();
        public List<NPCAssociation> NPCAssociations { get; set; } = new List<NPCAssociation>();
        public List<SceneTrigger> SceneTriggers { get; set; } = new List<SceneTrigger>();
        public bool IsLocked { get; set; } = true;
    }

    public enum QuestType
    {
        Main,
        Side
    }

    public abstract class QuestObjective
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public abstract bool IsCompleted(object progress);
    }

    public class TalkObjective : QuestObjective
    {
        public string TargetNpc { get; set; }

        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public class KillObjective : QuestObjective
    {
        public string TargetEnemy { get; set; }
        public int Count { get; set; }

        public override bool IsCompleted(object progress)
        {
            if (progress is int count)
            {
                return count >= Count;
            }
            return false;
        }
    }

    public class CollectObjective : QuestObjective
    {
        public string TargetItem { get; set; }
        public int Count { get; set; }
        public bool ConsumeOnComplete { get; set; }

        public override bool IsCompleted(object progress)
        {
            if (progress is int count)
            {
                return count >= Count;
            }
            return false;
        }
    }

    public class ReachLocationObjective : QuestObjective
    {
        public string TargetScene { get; set; }
        public Vector3Data Location { get; set; }
        public float Radius { get; set; } = 5.0f;

        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public class EscortObjective : QuestObjective
    {
        public string TargetNpc { get; set; }
        public string TargetScene { get; set; }

        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public class RescueObjective : QuestObjective
    {
        public int Count { get; set; }

        public override bool IsCompleted(object progress)
        {
            if (progress is int count)
            {
                return count >= Count;
            }
            return false;
        }
    }

    public class DefendObjective : QuestObjective
    {
        public string TargetLocation { get; set; }
        public int Duration { get; set; }

        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public class ProtectObjective : QuestObjective
    {
        public string TargetNpc { get; set; }

        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public class PrepareObjective : QuestObjective
    {
        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public class AttendObjective : QuestObjective
    {
        public string TargetEvent { get; set; }

        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public class DiscoverObjective : QuestObjective
    {
        public override bool IsCompleted(object progress)
        {
            return progress is bool v && v;
        }
    }

    public abstract class QuestReward
    {
        public string Type { get; set; }
        public abstract void Grant(QuestManager questManager);
    }

    public class ExperienceReward : QuestReward
    {
        public int Amount { get; set; }

        public override void Grant(QuestManager questManager)
        {
            questManager.GrantExperience(Amount);
        }
    }

    public class ItemReward : QuestReward
    {
        public string ItemId { get; set; }
        public int Count { get; set; }

        public override void Grant(QuestManager questManager)
        {
            questManager.GrantItem(ItemId, Count);
        }
    }

    public class CurrencyReward : QuestReward
    {
        public string CurrencyType { get; set; }
        public int Amount { get; set; }

        public override void Grant(QuestManager questManager)
        {
            questManager.GrantCurrency(CurrencyType, Amount);
        }
    }

    public class EquipmentReward : QuestReward
    {
        public string EquipmentId { get; set; }

        public override void Grant(QuestManager questManager)
        {
            questManager.GrantEquipment(EquipmentId);
        }
    }

    public class NPCAssociation
    {
        public string NPCId { get; set; }
        public List<NPCTalkType> TalkTypes { get; set; } = new List<NPCTalkType>();
    }

    public enum NPCTalkType
    {
        Accept,
        HandIn,
        Progress,
        Complete
    }

    public class SceneTrigger
    {
        public string SceneId { get; set; }
        public TriggerType TriggerType { get; set; }
        public string AnimationName { get; set; }
        public bool Skipable { get; set; }
    }

    public enum TriggerType
    {
        OnStart,
        OnComplete,
        OnObjective
    }

    public class Vector3Data
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}