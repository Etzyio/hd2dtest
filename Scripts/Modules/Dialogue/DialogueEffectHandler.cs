using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// Handles the execution of dialogue events/effects.
    /// </summary>
    public class DialogueEffectHandler
    {
        private Node _context; // The node context for running effects (e.g. DialogueUI)

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
                case "GiveItem":
                    HandleGiveItem(evt.Parameters);
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
                // In a real game, this would call AudioManager.PlaySound(soundId)
                Log.Info($"Playing Sound: {soundId}");
                
                // If context has an AudioStreamPlayer, try to play
                if (_context.HasNode("AudioPlayer"))
                {
                    var player = _context.GetNode<AudioStreamPlayer>("AudioPlayer");
                    // Load sound logic here
                }
            }
        }

        private void HandleScreenShake(Dictionary<string, string> parameters)
        {
            // Shake logic
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

        private void HandleAddQuest(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("quest_id", out string questId))
            {
                Log.Info($"Adding Quest: {questId}");
                // QuestManager.AddQuest(questId);
            }
        }

        private void HandleGiveItem(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("item_id", out string itemId))
            {
                Log.Info($"Giving Item: {itemId}");
                // InventoryManager.AddItem(itemId);
            }
        }
    }
}
