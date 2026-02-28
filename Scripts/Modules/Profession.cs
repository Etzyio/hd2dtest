using System;
using System.Collections.Generic;
using System.Linq;

namespace hd2dtest.Scripts.Modules
{
    public enum PassiveCategory
    {
        MainExclusive,
        SubExclusive,
        General
    }

    public class PassiveEffect
    {
        public string Stat = "Attack";
        public float Value = 0f;
        public string StackGroup = "";
    }

    public class PassiveSkillDef
    {
        public string Id = "";
        public string Name = "";
        public string Description = "";
        public PassiveCategory Category = PassiveCategory.General;
        public int Priority = 0;
        public List<string> ConflictsWith { get; set; } = [];
        public List<PassiveEffect> Effects { get; set; } = [];
    }

    public class Profession
    {
        public string Id = "";
        public string Name = "";
        public string Description = "";
        public List<PassiveSkillDef> Passives { get; set; } = [];
    }

    public class PassiveOwnership
    {
        public string PassiveId = "";
        public string SourceProfessionId = "";
        public bool Mastered = false;
    }

    public static class ProfessionRegistry
    {
        private static readonly Dictionary<string, Profession> _professions = new();
        private static readonly Dictionary<string, PassiveSkillDef> _passives = new();

        public static void RegisterProfession(Profession p)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.Id)) return;
            _professions[p.Id] = p;
            foreach (var ps in p.Passives)
            {
                _passives[ps.Id] = ps;
            }
        }

        public static Profession GetProfession(string id) =>
            id != null && _professions.TryGetValue(id, out var v) ? v : null;

        public static PassiveSkillDef GetPassive(string id) =>
            id != null && _passives.TryGetValue(id, out var v) ? v : null;
    }
}
