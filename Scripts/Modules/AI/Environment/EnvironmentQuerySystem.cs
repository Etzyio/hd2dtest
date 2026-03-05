using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.Environment
{
    [GlobalClass]
    public partial class CoverPoint : Node3D
    {
         public bool IsOccupied { get; set; } = false;
         public float Quality { get; set; } = 1.0f; // 0 to 1
        
        // Direction of cover (normal of the wall it provides cover from)
         public Vector3 CoverDirection { get; set; } = Vector3.Forward; 
    }

    [GlobalClass]
    public partial class EnvironmentQuerySystem : Node
    {
        public static EnvironmentQuerySystem Instance { get; private set; }
        
        private List<CoverPoint> _coverPoints = new List<CoverPoint>();

        public override void _Ready()
        {
            Instance = this;
            // Find all cover points in scene (or register manually)
            // For prototype, we assume they register themselves or we search group
        }

        public void RegisterCover(CoverPoint point)
        {
            if (!_coverPoints.Contains(point)) _coverPoints.Add(point);
        }

        public CoverPoint FindBestCover(Vector3 agentPos, Vector3 threatPos, float maxDistance = 20f)
        {
            CoverPoint bestCover = null;
            float bestScore = -1f;

            foreach (var cover in _coverPoints)
            {
                if (cover.IsOccupied) continue;

                float dist = agentPos.DistanceTo(cover.GlobalPosition);
                if (dist > maxDistance) continue;

                // Evaluate cover
                // 1. Distance score (closer is better, but not too close to threat)
                float distScore = 1.0f - (dist / maxDistance);
                
                // 2. Direction check (cover should block threat)
                Vector3 toThreat = (threatPos - cover.GlobalPosition).Normalized();
                float dot = toThreat.Dot(cover.CoverDirection);
                // If dot is positive, cover is facing threat (good if cover is a wall)
                // Assuming CoverDirection points OUT from the wall towards open space.
                // So if we are behind cover, threat is opposite to CoverDirection?
                // Usually CoverDirection is the normal of the surface.
                // If threat is in front of wall, dot > 0.
                
                // Simplified: Check if cover is between threat and agent?
                // Or just use dot product.
                float coverEffectiveness = Mathf.Max(0, dot); 

                float score = distScore + coverEffectiveness * 2.0f + cover.Quality;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCover = cover;
                }
            }

            return bestCover;
        }
    }
}
