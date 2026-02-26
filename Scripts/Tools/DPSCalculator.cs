using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.SkillSystem;

namespace hd2dtest.Scripts.Tools
{
    public class DPSCalculator
    {
        public struct SimulationResult
        {
            public float TotalDamage;
            public float DPS;
            public float Duration;
        }

        public static SimulationResult CalculateDPS(Creature attacker, Creature dummyTarget, SkillData skill, float duration = 60f)
        {
            float totalDamage = 0f;
            float currentTime = 0f;
            float skillCooldownTimer = 0f;

            // Simplified simulation loop
            while (currentTime < duration)
            {
                // Process cooldown
                if (skillCooldownTimer <= 0)
                {
                    // Cast skill
                    // Assuming skill execution is instant for DPS calculation or takes Phase duration
                    float skillExecutionTime = 0f;
                    foreach(var phase in skill.Phases)
                    {
                        skillExecutionTime += phase.Duration;
                        foreach(var evt in phase.Events)
                        {
                            if (evt is DamageSkillEvent dmgEvt)
                            {
                                float damage = attacker.Attack * dmgEvt.DamageMultiplier;
                                // Basic defense mitigation
                                float mitigation = dummyTarget.Defense / (dummyTarget.Defense + 100); 
                                damage *= (1.0f - mitigation);
                                totalDamage += damage;
                            }
                        }
                    }

                    // Add execution time
                    currentTime += skillExecutionTime;
                    // Start cooldown
                    skillCooldownTimer = skill.Cooldown;
                }
                else
                {
                    // Advance time
                    float step = 0.1f;
                    currentTime += step;
                    skillCooldownTimer -= step;
                }
            }

            return new SimulationResult
            {
                TotalDamage = totalDamage,
                Duration = duration,
                DPS = totalDamage / duration
            };
        }
    }
}
