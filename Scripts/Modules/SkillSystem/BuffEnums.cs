/*
 * File: BuffEnums.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose: Buff 系统的枚举类型定义。
 */

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    public enum BuffEffectType
    {
        AttackBonus,
        DefenseBonus,
        SpeedBonus,
        HealthRegeneration,
        ManaRegeneration,
        DamageReduction,
        CriticalRateBonus,
        StatusResistance
    }

    public enum BuffDurationType
    {
        Instant,
        Duration,
        Turns,
        Permanent
    }
}
