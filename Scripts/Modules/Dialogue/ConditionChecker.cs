using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// Evaluates dialogue option conditions against actual game state.
    /// Condition format: "Type:Key:Value"
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
                    return CheckHasItem(key);
                case "QuestState":
                    return CheckQuestState(key, value);
                case "NPCState":
                    return CheckNPCState(key, value);
                case "HasTeammate":
                    return CheckHasTeammate(key);
                case "Level":
                    return CheckLevel(key);
                case "Gold":
                    return CheckGold(key);
                case "StoryFlag":
                    return CheckStoryFlag(key, value);
                default:
                    Log.Warning($"Unknown condition type: {type}");
                    return false;
            }
        }

        private static bool CheckHasItem(string itemId)
        {
            var itemList = GameDataManager.Instance?.ItemList;
            if (itemList == null) return false;
            bool has = itemList.ContainsKey(itemId) && itemList[itemId] > 0;
            Log.Info($"Condition HasItem:{itemId} = {has}");
            return has;
        }

        private static bool CheckQuestState(string questId, string expectedState)
        {
            var qm = QuestManager.Instance;
            if (qm == null) return false;
            var status = qm.GetQuestStatus(questId);
            bool matches = status.ToString().Equals(expectedState, StringComparison.OrdinalIgnoreCase);
            Log.Info($"Condition QuestState:{questId}:{expectedState} = {matches} (actual: {status})");
            return matches;
        }

        private static bool CheckNPCState(string npcId, string expectedStatus)
        {
            var npcStatus = GameDataManager.Instance?.NPCStatus;
            if (npcStatus == null) return false;
            bool matches = npcStatus.TryGetValue(npcId, out int current) &&
                           current.ToString() == expectedStatus;
            Log.Info($"Condition NPCState:{npcId}:{expectedStatus} = {matches}");
            return matches;
        }

        private static bool CheckHasTeammate(string teammateId)
        {
            var teammates = GameDataManager.Instance?.Teammates;
            if (teammates == null) return false;
            var list = teammates.Get();
            bool has = list.Exists(t => t.CreatureName == teammateId);
            Log.Info($"Condition HasTeammate:{teammateId} = {has}");
            return has;
        }

        private static bool CheckLevel(string minLevelStr)
        {
            if (!int.TryParse(minLevelStr, out int minLevel)) return false;
            var player = GameDataManager.Instance?.Teammates?.Player;
            if (player == null) return false;
            bool meets = player.Level >= minLevel;
            Log.Info($"Condition Level:{minLevel} = {meets} (player level: {player.Level})");
            return meets;
        }

        private static bool CheckGold(string minGoldStr)
        {
            if (!int.TryParse(minGoldStr, out int minGold)) return false;
            var player = GameDataManager.Instance?.Teammates?.Player;
            if (player == null) return false;
            bool meets = player.Gold >= minGold;
            Log.Info($"Condition Gold:{minGold} = {meets} (player gold: {player.Gold})");
            return meets;
        }

        private static bool CheckStoryFlag(string flagKey, string expectedValue)
        {
            string current = GameDataManager.Instance?.GetStoryFlag(flagKey);
            bool matches = current == expectedValue;
            Log.Info($"Condition StoryFlag:{flagKey}:{expectedValue} = {matches} (actual: {current})");
            return matches;
        }
    }
}
