using Godot;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.Dialogue;
using hd2dtest.Scripts.Quest;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.NPC
{
    /// <summary>
    /// Placeable NPC scene component. Attach to a CharacterBody3D node in the editor.
    /// Resolves dialogue based on quest status and NPC story state.
    /// </summary>
    public partial class MapNPC : Scripts.Modules.NPC
    {
        [Export] public string NpcDataId { get; set; }
        [Export] public string DefaultDialogueGraph { get; set; }
        [Export] public string InteractionHint { get; set; } = "npc_interact_hint";

        private Area3D _interactionArea;
        private CollisionShape3D _interactionShape;
        private Label3D _hintLabel;
        private bool _playerInRange;

        public override void _Ready()
        {
            AddToGroup("map_npc");
            base.Initialize();

            if (!string.IsNullOrEmpty(NpcDataId))
                LoadNpcData();

            SetupInteractionArea();
            CreateHintLabel();

            // Enable input processing for this node
            SetProcessInput(true);
        }

        public override void _Process(double delta)
        {
            if (!IsAlive) return;
            UpdateAI((float)delta);
            UpdateHintLabel();
        }

        public override void _Input(InputEvent @event)
        {
            if (!_playerInRange || !@event.IsActionPressed("interact"))
                return;

            // Only the closest MapNPC to the player responds to the interact key
            var player = FindPlayer();
            if (player == null) return;

            if (!IsClosestMapNPCTo(player))
                return;

            InteractWithPlayer();
            GetViewport().SetInputAsHandled();
        }

        /// <summary>
        /// Returns true if this MapNPC is the closest one to the given player.
        /// Prevents multiple NPCs from responding to a single interact press.
        /// </summary>
        private bool IsClosestMapNPCTo(Scripts.Modules.Creature player)
        {
            float myDist = GlobalPosition.DistanceSquaredTo(player.GlobalPosition);
            foreach (var node in GetTree().GetNodesInGroup("map_npc"))
            {
                if (node == this) continue;
                if (node is MapNPC other && other._playerInRange)
                {
                    float otherDist = other.GlobalPosition.DistanceSquaredTo(player.GlobalPosition);
                    if (otherDist < myDist) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Loads NPC properties from NPCs.json static data.
        /// </summary>
        private void LoadNpcData()
        {
            if (!ResourcesManager.NPCsCache.TryGetValue(NpcDataId, out Scripts.Modules.NPC template))
                return;

            NPCId = template.NPCId;
            Type = template.Type;
            CreatureName = template.CreatureName;
            Dialogue = template.Dialogue;
            DialogueGraphId = template.DialogueGraphId;
            IsInteractive = template.IsInteractive;
            AvailableInteractions = template.AvailableInteractions;
            DetectionRadius = template.DetectionRadius;

            Log.Info($"MapNPC {NpcDataId} loaded: {CreatureName}");
        }

        private void SetupInteractionArea()
        {
            _interactionArea = new Area3D { Name = "InteractionArea" };
            _interactionShape = new CollisionShape3D
            {
                Shape = new SphereShape3D { Radius = DetectionRadius > 0 ? DetectionRadius : 3f }
            };
            _interactionArea.AddChild(_interactionShape);
            AddChild(_interactionArea);

            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;
        }

        private void CreateHintLabel()
        {
            _hintLabel = new Label3D
            {
                Name = "HintLabel",
                Text = TranslationServer.Translate(InteractionHint),
                Position = new Vector3(0, 2.5f, 0),
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                Visible = false,
                Modulate = new Color(1f, 1f, 0.8f, 1f),
                FontSize = 32
            };
            AddChild(_hintLabel);
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body.IsInGroup("player"))
            {
                _playerInRange = true;
                if (_hintLabel != null)
                    _hintLabel.Visible = true;
            }
        }

        private void OnBodyExited(Node3D body)
        {
            if (body.IsInGroup("player"))
            {
                _playerInRange = false;
                if (_hintLabel != null)
                    _hintLabel.Visible = false;
            }
        }

        private void UpdateHintLabel()
        {
            if (_hintLabel == null || !_hintLabel.Visible) return;

            // Bob the hint label up and down
            float bob = Mathf.Sin((float)Time.GetTicksMsec() / 1000.0f * 2f) * 0.1f;
            _hintLabel.Position = new Vector3(0, 2.5f + bob, 0);
        }

        private void InteractWithPlayer()
        {
            // Find the player in the scene
            var player = FindPlayer();
            if (player == null) return;

            var dialoguePath = ResolveActiveDialogue();
            if (!string.IsNullOrEmpty(dialoguePath))
            {
                CurrentState = NPCState.Talking;
                DialogueManager.Instance?.StartDialogue(dialoguePath);
            }
            else
            {
                Interact(player, InteractionType.Talk);
            }
        }

        private Scripts.Modules.Creature FindPlayer()
        {
            foreach (var node in GetTree().GetNodesInGroup("player"))
            {
                if (node is Scripts.Modules.Creature creature && creature.IsAlive)
                    return creature;
            }
            return null;
        }

        /// <summary>
        /// Resolves which dialogue graph to use based on current game state:
        /// 1. Active/completable quests involving this NPC
        /// 2. NPC story state (NPCStatus)
        /// 3. Default dialogue graph
        /// </summary>
        public string ResolveActiveDialogue()
        {
            // Check for quest-related dialogue first
            var questList = GameDataManager.Instance?.QuestList;
            if (questList != null)
            {
                foreach (var (questId, questData) in questList)
                {
                    if (questData.NPCAssociations == null) continue;
                    var assoc = questData.NPCAssociations.Find(a => a.NPCId == NPCId);
                    if (assoc == null) continue;

                    var status = QuestManager.Instance?.GetQuestStatus(questId);
                    if (status == QuestManager.QuestStatus.InProgress && assoc.TalkTypes.Contains(NPCTalkType.Progress))
                        return $"res://Resources/Dialogues/quest_{questId}_progress.json";
                    if (status == QuestManager.QuestStatus.Completed && assoc.TalkTypes.Contains(NPCTalkType.HandIn))
                        return $"res://Resources/Dialogues/quest_{questId}_handin.json";
                    if (status == QuestManager.QuestStatus.Available && assoc.TalkTypes.Contains(NPCTalkType.Accept))
                        return $"res://Resources/Dialogues/quest_{questId}_accept.json";
                }
            }

            // Check NPC story state for conditional dialogue
            var npcStatus = GameDataManager.Instance?.NPCStatus;
            if (npcStatus != null && npcStatus.TryGetValue(NPCId, out int state))
            {
                var statePath = $"res://Resources/Dialogues/{NPCId}_state_{state}.json";
                if (ResourceLoader.Exists(statePath))
                    return statePath;
            }

            // Fall back to default
            if (!string.IsNullOrEmpty(DefaultDialogueGraph))
                return DefaultDialogueGraph;

            if (!string.IsNullOrEmpty(DialogueGraphId))
                return DialogueGraphId;

            return null;
        }
    }
}
