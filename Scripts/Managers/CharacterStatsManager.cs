using Godot;
using System;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    public partial class CharacterStatsManager : Node
    {
        public static CharacterStatsManager Instance { get; private set; }

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                Log.Info("CharacterStatsManager initialized");
            }
            else
            {
                Log.Warning("CharacterStatsManager instance already exists");
                QueueFree();
            }
        }

        public void CalculateStats(Creature creature)
        {
            if (creature == null)
            {
                Log.Error("Creature is null");
                return;
            }

            float baseHealth = 100f + (creature.Level - 1) * 15f;
            float baseAttack = 10f + (creature.Level - 1) * 2f;
            float baseDefense = 5f + (creature.Level - 1) * 1f;
            float baseSpeed = 50f + (creature.Level - 1) * 2f;

            float equipmentAttack = 0f;
            float equipmentDefense = 0f;
            float equipmentHealth = 0f;

            if (creature is hd2dtest.Scripts.Modules.Player player)
            {
                if (player.CurrentWeapon != null)
                {
                    equipmentAttack += player.CurrentWeapon.AttackPower;
                }

                foreach (var eq in player.Equipments)
                {
                    equipmentDefense += eq.Defense;
                    equipmentHealth += eq.Health;
                }

                foreach (var passive in player.EquippedPassives)
                {
                    baseHealth += passive.HealthBonus;
                    baseAttack += passive.AttackBonus;
                    baseDefense += passive.DefenseBonus;
                    baseSpeed += passive.SpeedBonus;
                }
            }

            creature.MaxHealth = baseHealth + equipmentHealth;
            creature.Attack = baseAttack + equipmentAttack;
            creature.Defense = baseDefense + equipmentDefense;
            creature.Speed = baseSpeed;

            if (creature.Health > creature.MaxHealth)
            {
                creature.Health = creature.MaxHealth;
            }

            Log.Info($"Stats calculated for {creature.CreatureName}: HP={creature.MaxHealth}, ATK={creature.Attack}, DEF={creature.Defense}, SPD={creature.Speed}");
        }

        public void ApplyLevelUp(Creature creature)
        {
            if (creature == null)
            {
                Log.Error("Creature is null");
                return;
            }

            float healthGain = 15f;
            float attackGain = 2f;
            float defenseGain = 1f;
            float speedGain = 2f;

            if (creature is hd2dtest.Scripts.Modules.Player player && player.MainClass != null)
            {
                healthGain *= player.MainClass.HealthGrowth;
                attackGain *= player.MainClass.AttackGrowth;
                defenseGain *= player.MainClass.DefenseGrowth;
                speedGain *= player.MainClass.SpeedGrowth;
            }

            creature.MaxHealth += healthGain;
            creature.Health += healthGain;
            creature.Attack += attackGain;
            creature.Defense += defenseGain;
            creature.Speed += speedGain;

            Log.Info($"{creature.CreatureName} leveled up! Gained +{healthGain} HP, +{attackGain} ATK, +{defenseGain} DEF, +{speedGain} SPD");
        }

        public int CalculateDamage(Creature attacker, Creature target)
        {
            if (attacker == null || target == null)
            {
                Log.Error("Attacker or target is null");
                return 0;
            }

            float baseDamage = attacker.Attack;
            float defense = target.Defense;

            float damage = Mathf.Max(1, baseDamage - defense * 0.5f);

            float randomFactor = (float)GD.RandRange(0.9, 1.1);
            damage *= randomFactor;

            return Mathf.FloorToInt(damage);
        }

        public int CalculateHealing(Creature healer, float healAmount)
        {
            if (healer == null)
            {
                Log.Error("Healer is null");
                return 0;
            }

            float healing = healAmount;

            return Mathf.FloorToInt(healing);
        }

        public float CalculateDamageReduction(Creature creature)
        {
            if (creature == null)
            {
                Log.Error("Creature is null");
                return 0f;
            }

            float reduction = creature.Defense * 0.02f;
            return Mathf.Clamp(reduction, 0f, 0.5f);
        }

        public int CalculateExperienceReward(Creature defeated)
        {
            if (defeated == null)
            {
                Log.Error("Defeated creature is null");
                return 0;
            }

            return defeated.Level * 10;
        }

        public int CalculateGoldReward(Creature defeated)
        {
            if (defeated == null)
            {
                Log.Error("Defeated creature is null");
                return 0;
            }

            return defeated.Level * 5 + (int)GD.RandRange(1, 10);
        }
    }
}