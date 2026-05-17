/*
 * File: Player.ClassProficiency.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose: 职业精通度数据类，记录角色在某个职业上的熟练程度。
 */

using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules
{
    public class ClassProficiency
    {
        public string ProfessionId { get; set; } = "";
        public bool IsUnlocked { get; set; } = false;
        public int ProficiencyLevel { get; set; } = 0;
        public List<string> LearnedSkillIds { get; set; } = new();
    }
}
