using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules; // For Creature

namespace hd2dtest.Scripts.Modules.AI
{
    [GlobalClass]
    public partial class SensorySystem : Node3D
    {
        public float VisionRange { get; set; } = 15.0f;
        public float VisionAngle { get; set; } = 90.0f; // Degrees
        public float HearingRange { get; set; } = 20.0f;
        public uint DetectionLayer { get; set; } = 1; // Default layer

        private List<Node3D> _visibleTargets = new List<Node3D>();
        private List<Node3D> _audibleTargets = new List<Node3D>();

        public IReadOnlyList<Node3D> VisibleTargets => _visibleTargets;
        public IReadOnlyList<Node3D> AudibleTargets => _audibleTargets;

        public override void _PhysicsProcess(double delta)
        {
            UpdateVision();
            UpdateHearing();
        }

        private void UpdateVision()
        {
            _visibleTargets.Clear();

            // Optimization: In real game, use a specific Area3D or spatial query
            var potentialTargets = GetTree().GetNodesInGroup("Player");

            foreach (Node node in potentialTargets)
            {
                if (node is Node3D targetNode)
                {
                    if (IsVisible(targetNode))
                    {
                        _visibleTargets.Add(targetNode);
                    }
                }
            }
        }

        private bool IsVisible(Node3D target)
        {
            float dist = GlobalPosition.DistanceTo(target.GlobalPosition);
            if (dist > VisionRange) return false;

            Vector3 directionToTarget = (target.GlobalPosition - GlobalPosition).Normalized();
            Vector3 forward = -GlobalTransform.Basis.Z; // Godot forward is -Z
            float angle = Mathf.RadToDeg(forward.AngleTo(directionToTarget));

            if (angle > VisionAngle / 2.0f) return false;

            // Raycast for occlusion
            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(GlobalPosition, target.GlobalPosition);
            query.CollisionMask = DetectionLayer;

            var result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                Node collider = (Node)result["collider"];
                // Check if collider is the target or a child of target
                if (collider == target || target.IsAncestorOf(collider))
                {
                    return true;
                }
                return false; // Obstructed by something else
            }

            // If ray hits nothing, it means path is clear of obstacles on DetectionLayer.
            // Assuming target is also on DetectionLayer, it should have been hit.
            // If target is NOT on DetectionLayer, but we only care about walls (which are on DetectionLayer),
            // then no hit means visible.
            // Let's assume we want to be strict: must hit target.
            // But for robustness, if we only mask static geometry, no hit = visible.
            // If we mask everything, we expect hit.
            // Let's assume 'DetectionLayer' includes obstacles.
            return true;
        }

        private void UpdateHearing()
        {
            _audibleTargets.Clear();
            var potentialTargets = GetTree().GetNodesInGroup("Player");
            foreach (Node node in potentialTargets)
            {
                if (node is Node3D targetNode)
                {
                    if (GlobalPosition.DistanceTo(targetNode.GlobalPosition) <= HearingRange)
                    {
                        _audibleTargets.Add(targetNode);
                    }
                }
            }
        }

        public void OnSoundHeard(Vector3 location, float volume)
        {
            if (GlobalPosition.DistanceTo(location) <= HearingRange)
            {
                // Logic to react to sound
            }
        }
    }
}
