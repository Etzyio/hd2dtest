using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Modules.AI.BehaviorTree;
using hd2dtest.Scripts.Modules.AI.BehaviorTree.Actions;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Tests.Performance
{
    public partial class AIBenchmark : Node3D
    {
        [Export] public int AgentCount { get; set; } = 80;
        
        private List<Node3D> _agents = new List<Node3D>();
        private Node3D _target;
        private Stopwatch _stopwatch = new Stopwatch();

        public override void _Ready()
        {
            SetupScene();
            SetupAgents();
        }

        private void SetupScene()
        {
            // Create floor
            var floor = new CSGBox3D();
            floor.Size = new Vector3(100, 1, 100);
            floor.UseCollision = true;
            AddChild(floor);

            // Create target
            _target = new Marker3D();
            _target.Name = "Target";
            AddChild(_target);
            
            // Create camera
            var cam = new Camera3D();
            cam.Position = new Vector3(0, 50, 0);
            cam.RotationDegrees = new Vector3(-90, 0, 0);
            AddChild(cam);
        }

        private void SetupAgents()
        {
            for (int i = 0; i < AgentCount; i++)
            {
                var agent = new CharacterBody3D();
                agent.Name = $"Agent_{i}";
                
                // Add visual
                var mesh = new MeshInstance3D();
                mesh.Mesh = new CapsuleMesh();
                agent.AddChild(mesh);
                
                // Add collision
                var col = new CollisionShape3D();
                col.Shape = new CapsuleShape3D();
                agent.AddChild(col);

                // Add BT Runner
                var runner = new BehaviorTreeRunner();
                runner.Name = "BTRunner";
                agent.AddChild(runner);

                // Build simple BT: Sequence(DetectTarget, MoveToTarget)
                // Note: DetectTarget needs SensorySystem, skipping for raw BT overhead test
                // Just testing MoveToTarget logic update
                
                var root = new Sequence();
                // Mock blackboard target
                runner.SetBlackboardValue("Target", _target);
                
                var moveAction = new MoveToTarget(agent, "Target", 5.0f, 2.0f);
                root.AddChild(moveAction);
                
                runner.SetRoot(root);
                
                // Random position
                agent.Position = new Vector3(GD.Randf() * 80 - 40, 1, GD.Randf() * 80 - 40);
                
                AddChild(agent);
                _agents.Add(agent);
            }
        }

        public override void _Process(double delta)
        {
            // Move target in circle
            float time = Time.GetTicksMsec() / 1000.0f;
            _target.Position = new Vector3(Mathf.Sin(time) * 30, 1, Mathf.Cos(time) * 30);

            // Measure frame time
            // Note: This measures full frame, including physics and rendering.
            // For pure AI overhead, we'd need to profile BTRunner._Process separately or use Profiler.
            // But checking FPS drop is a good integration test.
            if (Engine.GetFramesPerSecond() < 55)
            {
                // Log.Warning($"FPS drop detected: {Engine.GetFramesPerSecond()}");
            }
        }
    }
}
