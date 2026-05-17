using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// Executes dialogue events with real game manager integration.
    /// </summary>
    public class DialogueEffectHandler
    {
        private Node _context;

        public DialogueEffectHandler(Node context)
        {
            _context = context;
        }

        public void HandleEvent(DialogueEvent evt)
        {
            Log.Info($"Handling Dialogue Event: {evt.Type}");

            switch (evt.Type)
            {
                case "PlaySound":
                    HandlePlaySound(evt.Parameters);
                    break;
                case "ScreenShake":
                    HandleScreenShake(evt.Parameters);
                    break;
                case "AddQuest":
                    HandleAddQuest(evt.Parameters);
                    break;
                case "CompleteQuest":
                    HandleCompleteQuest(evt.Parameters);
                    break;
                case "GiveItem":
                    HandleGiveItem(evt.Parameters);
                    break;
                case "AddTeammate":
                    HandleAddTeammate(evt.Parameters);
                    break;
                case "SetNPCStatus":
                    HandleSetNPCStatus(evt.Parameters);
                    break;
                case "StartBattle":
                    HandleStartBattle(evt.Parameters);
                    break;
                case "SetStoryFlag":
                    HandleSetStoryFlag(evt.Parameters);
                    break;
                default:
                    Log.Warning($"Unknown event type: {evt.Type}");
                    break;
            }
        }

        private void HandlePlaySound(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("sound_id", out string soundId))
            {
                Log.Info($"Playing Sound: {soundId}");
                // AudioManager integration point
            }
        }

        private void HandleScreenShake(Dictionary<string, string> parameters)
        {
            if (_context is Control control)
            {
                var tween = control.CreateTween();
                Vector2 originalPos = control.Position;
                for (int i = 0; i < 5; i++)
                {
                    tween.TweenProperty(control, "position", originalPos + new Vector2(GD.RandRange(-5, 5), GD.RandRange(-5, 5)), 0.05f);
                    tween.TweenProperty(control, "position", originalPos, 0.05f);
                }
            }
        }

        private static void HandleAddQuest(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("quest_id", out string questId))
            {
                Log.Info($"Adding Quest: {questId}");
                QuestManager.Instance?.StartQuest(questId);
            }
        }

        private static void HandleCompleteQuest(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("quest_id", out string questId))
            {
                Log.Info($"Completing Quest: {questId}");
                QuestManager.Instance?.CompleteQuest(questId);
            }
        }

        private static void HandleGiveItem(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("item_id", out string itemId))
            {
                int count = 1;
                if (parameters.TryGetValue("count", out string countStr))
                    int.TryParse(countStr, out count);

                Log.Info($"Giving Item: {itemId} x{count}");
                var item = GameDataManager.Instance?.GetItemById(itemId);
                if (item != null)
                {
                    for (int i = 0; i < count; i++)
                        GameDataManager.Instance?.AddItem(item);
                }
            }
        }

        private static void HandleAddTeammate(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("teammate_id", out string teammateId))
            {
                Log.Info($"Adding Teammate: {teammateId}");
                // NPCs can join as Player-type teammates
                if (ResourcesManager.NPCsCache.TryGetValue(teammateId, out NPC npc))
                {
                    var player = new Player
                    {
                        CreatureName = npc.CreatureName,
                        Level = npc.Level,
                        Health = npc.MaxHealth,
                        MaxHealth = npc.MaxHealth,
                        Attack = npc.Attack,
                        Defense = npc.Defense,
                        Speed = npc.Speed
                    };
                    GameDataManager.Instance?.Teammates.Add(player);
                }
            }
        }

        private static void HandleSetNPCStatus(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("npc_id", out string npcId) &&
                parameters.TryGetValue("status", out string statusStr) &&
                int.TryParse(statusStr, out int status))
            {
                Log.Info($"Setting NPCStatus: {npcId} = {status}");
                var npcStatus = GameDataManager.Instance?.NPCStatus;
                if (npcStatus != null)
                    npcStatus[npcId] = status;
            }
        }

        private static void HandleStartBattle(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("battle_id", out string battleId))
            {
                Log.Info($"Starting Battle: {battleId}");
                hd2dtest.Scenes.BattleScene.PendingBattleData = (battleId, Main.Instance?.CurrentSceneName ?? "start");
                Main.Instance?.SwitchScene("battle");
            }
        }

        private static void HandleSetStoryFlag(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("flag_key", out string key) &&
                parameters.TryGetValue("flag_value", out string value))
            {
                Log.Info($"Setting StoryFlag: {key} = {value}");
                GameDataManager.Instance?.SetStoryFlag(key, value);
            }
        }
    }
}
