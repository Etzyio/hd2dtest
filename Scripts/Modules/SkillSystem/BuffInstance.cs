/*
 * File: BuffInstance.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose: 运行时 Buff 实例，管理单个 Buff 的状态。
 */

using Godot;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    public class BuffInstance
    {
        public BuffData Data { get; set; }
        public Creature Applier { get; set; }
        public Creature Target { get; set; }
        public int CurrentStacks { get; set; } = 1;
        public float RemainingTime { get; set; }
        public int RemainingTurns { get; set; }
        public float ApplyTime { get; set; }

        public bool IsExpired =>
            Data.DurationType == BuffDurationType.Duration && RemainingTime <= 0f ||
            Data.DurationType == BuffDurationType.Turns && RemainingTurns <= 0;

        public BuffInstance(BuffData data, Creature applier, Creature target)
        {
            Data = data;
            Applier = applier;
            Target = target;
            RemainingTime = data.Duration;
            RemainingTurns = data.TurnCount;
            ApplyTime = Time.GetTicksMsec() / 1000.0f;
        }

        public void Update(float delta)
        {
            if (Data.DurationType == BuffDurationType.Duration)
                RemainingTime -= delta;
        }

        public void UpdateTurn()
        {
            if (Data.DurationType == BuffDurationType.Turns)
                RemainingTurns--;
        }

        public bool Stack()
        {
            if (CurrentStacks >= Data.MaxStacks)
            {
                if (Data.RefreshDurationOnReapply)
                {
                    RemainingTime = Data.Duration;
                    RemainingTurns = Data.TurnCount;
                }
                return false;
            }

            CurrentStacks++;
            RemainingTime = Data.Duration;
            RemainingTurns = Data.TurnCount;
            return true;
        }
    }
}
