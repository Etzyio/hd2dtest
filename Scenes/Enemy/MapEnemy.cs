using Godot;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.Enemy
{
    /// <summary>
    /// Placeable enemy on maps. Roams with patrol AI and triggers battle on player contact.
    /// </summary>
    public partial class MapEnemy : CharacterBody3D
    {
        [Export] public string MonsterId { get; set; }
        [Export] public string BattleGroupId { get; set; }
        [Export] public float PatrolRadius { get; set; } = 5f;
        [Export] public float DetectionRadius { get; set; } = 4f;
        [Export] public float MoveSpeed { get; set; } = 2f;

        private Area3D _detectionArea;
        private CollisionShape3D _bodyShape;
        private Sprite3D _sprite;
        private Vector3 _startPosition;
        private Vector3 _patrolTarget;
        private float _patrolTimer;
        private bool _playerDetected;

        public override void _Ready()
        {
            _startPosition = Position;
            SetupBody();
            SetupDetection();
            SetupVisual();
            PickNewPatrolTarget();
        }

        public override void _Process(double delta)
        {
            if (!_playerDetected)
            {
                Patrol((float)delta);
            }
        }

        private void SetupBody()
        {
            _bodyShape = new CollisionShape3D
            {
                Shape = new CapsuleShape3D { Radius = 0.5f, Height = 2f }
            };
            AddChild(_bodyShape);
        }

        private void SetupDetection()
        {
            _detectionArea = new Area3D { Name = "DetectionArea" };
            var detectionShape = new CollisionShape3D
            {
                Shape = new SphereShape3D { Radius = DetectionRadius }
            };
            _detectionArea.AddChild(detectionShape);
            AddChild(_detectionArea);

            _detectionArea.BodyEntered += OnDetectionBodyEntered;
        }

        private void SetupVisual()
        {
            _sprite = new Sprite3D
            {
                Name = "EnemySprite",
                Position = new Vector3(0, 1, 0),
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
            };
            AddChild(_sprite);

            // Load monster data for visual
            if (!string.IsNullOrEmpty(MonsterId))
            {
                LoadMonsterData();
            }
        }

        private void LoadMonsterData()
        {
            if (ResourcesManager.MonstersCache.TryGetValue(MonsterId, out Monster template))
            {
                Log.Info($"MapEnemy loaded: {template.CreatureName} Lv.{template.Level}");
            }
        }

        private void OnDetectionBodyEntered(Node3D body)
        {
            if (_playerDetected) return;
            if (!body.IsInGroup("player")) return;

            _playerDetected = true;
            Log.Info($"MapEnemy {MonsterId}: Player detected, starting battle");

            // Set pending battle data and transition to battle scene
            hd2dtest.Scenes.BattleScene.PendingBattleData = (
                BattleGroupId ?? MonsterId,
                Main.Instance?.CurrentSceneName ?? "start"
            );
            Main.Instance?.SwitchScene("battle");
        }

        private void Patrol(float delta)
        {
            _patrolTimer -= delta;

            if (Position.DistanceTo(_patrolTarget) < 0.5f || _patrolTimer <= 0)
            {
                PickNewPatrolTarget();
            }

            Vector3 direction = (_patrolTarget - Position).Normalized();
            Velocity = direction * MoveSpeed;
            MoveAndSlide();
        }

        private void PickNewPatrolTarget()
        {
            float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
            float distance = (float)GD.RandRange(1f, PatrolRadius);
            _patrolTarget = _startPosition + new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance
            );
            _patrolTimer = (float)GD.RandRange(2f, 5f);
        }
    }
}
