using Godot;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.World
{
    /// <summary>
    /// Generic map trigger component. Attach to an Area node to handle
    /// scene transitions, battle starts, dialogue triggers, and quest updates.
    /// </summary>
    public partial class MapTrigger : Area3D
    {
        public enum MapTriggerType
        {
            SceneTransition,
            StartBattle,
            StartDialogue,
            QuestUpdate
        }

        [Export] public MapTriggerType TriggerType { get; set; } = MapTriggerType.SceneTransition;
        [Export] public string TargetId { get; set; }
        [Export] public string Condition { get; set; }
        [Export] public bool OneShot { get; set; } = true;
        [Export] public bool AutoTrigger { get; set; } = true;
        [Export] public string InteractHint { get; set; }

        private bool _triggered;
        private bool _playerInRange;
        private Label3D _hintLabel;

        public override void _Ready()
        {
            if (GetChildCount() == 0)
            {
                var shape = new CollisionShape3D
                {
                    Shape = new BoxShape3D { Size = new Vector3(2, 2, 2) }
                };
                AddChild(shape);
            }

            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;

            if (!AutoTrigger)
                CreateHintLabel();
        }

        public override void _Input(InputEvent @event)
        {
            if (!AutoTrigger && _playerInRange && @event.IsActionPressed("interact"))
            {
                TryExecute();
                GetViewport().SetInputAsHandled();
            }
        }

        private void OnBodyEntered(Node3D body)
        {
            if (!body.IsInGroup("player")) return;
            _playerInRange = true;

            if (AutoTrigger)
                TryExecute();

            if (_hintLabel != null)
                _hintLabel.Visible = true;
        }

        private void OnBodyExited(Node3D body)
        {
            if (!body.IsInGroup("player")) return;
            _playerInRange = false;

            if (_hintLabel != null)
                _hintLabel.Visible = false;
        }

        private void TryExecute()
        {
            if (OneShot && _triggered) return;
            if (!CheckCondition()) return;

            _triggered = true;
            Execute();
        }

        private bool CheckCondition()
        {
            if (string.IsNullOrEmpty(Condition)) return true;

            var checker = new Scripts.Modules.Dialogue.ConditionChecker();
            return checker.CheckCondition(Condition);
        }

        private void Execute()
        {
            Log.Info($"MapTrigger executing: {TriggerType} -> {TargetId}");

            switch (TriggerType)
            {
                case MapTriggerType.SceneTransition:
                    Main.Instance?.SwitchScene(TargetId);
                    break;

                case MapTriggerType.StartBattle:
                    Scenes.BattleScene.PendingBattleData = (TargetId, Main.Instance?.CurrentSceneName ?? "start");
                    Main.Instance?.SwitchScene("battle");
                    break;

                case MapTriggerType.StartDialogue:
                    Scripts.Managers.DialogueManager.Instance?.StartDialogue(TargetId);
                    break;

                case MapTriggerType.QuestUpdate:
                    if (TargetId.Contains(":"))
                    {
                        var parts = TargetId.Split(':');
                        Scripts.Quest.QuestManager.Instance?.UpdateQuestProgress(parts[0], parts.Length > 1 ? parts[1] : "default", 1);
                    }
                    break;
            }
        }

        private void CreateHintLabel()
        {
            _hintLabel = new Label3D
            {
                Text = TranslationServer.Translate(InteractHint ?? "npc_interact_hint"),
                Position = new Vector3(0, 1.5f, 0),
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                Visible = false,
                Modulate = new Color(1f, 1f, 0.5f, 1f),
                FontSize = 28
            };
            AddChild(_hintLabel);
        }
    }
}
