using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// Handles condition checking for dialogue options.
    /// </summary>
    public class ConditionChecker
    {
        public bool CheckCondition(string conditionString)
        {
            if (string.IsNullOrEmpty(conditionString)) return true;

            string[] parts = conditionString.Split(':');
            if (parts.Length < 2)
            {
                Log.Warning($"Invalid condition format: {conditionString}");
                return false;
            }

            string type = parts[0];
            string key = parts[1];
            string value = parts.Length > 2 ? parts[2] : "";

            switch (type)
            {
                case "HasItem":
                    // return InventoryManager.HasItem(key);
                    Log.Info($"Checking condition HasItem: {key}");
                    return true; // Stub
                case "QuestState":
                    // return QuestManager.GetQuestState(key) == value;
                    Log.Info($"Checking condition QuestState: {key} == {value}");
                    return true; // Stub
                default:
                    Log.Warning($"Unknown condition type: {type}");
                    return false;
            }
        }
    }
}
