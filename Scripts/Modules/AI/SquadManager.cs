using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules; // For Creature

namespace hd2dtest.Scripts.Modules.AI
{
    [GlobalClass]
    public partial class SquadManager : Node
    {
        public static SquadManager Instance { get; private set; }
        
        private List<Squad> _squads = new List<Squad>();

        public override void _Ready()
        {
            Instance = this;
        }

        public Squad CreateSquad(Node3D leader)
        {
            var squad = new Squad(leader);
            _squads.Add(squad);
            return squad;
        }

        public void RegisterMember(Node3D member, Squad squad)
        {
            if (squad != null && !_squads.Contains(squad))
            {
                _squads.Add(squad);
            }
            squad?.AddMember(member);
        }
    }

    public class Squad
    {
        public Node3D Leader { get; private set; }
        public List<Node3D> Members { get; private set; } = new List<Node3D>();
        public Vector3 TargetLocation { get; set; }
        public Node3D TargetEntity { get; set; }

        public Squad(Node3D leader)
        {
            Leader = leader;
            Members.Add(leader);
        }

        public void AddMember(Node3D member)
        {
            if (!Members.Contains(member))
            {
                Members.Add(member);
            }
        }

        public void RemoveMember(Node3D member)
        {
            Members.Remove(member);
            if (member == Leader)
            {
                // Elect new leader or disband
                if (Members.Count > 0)
                {
                    Leader = Members[0];
                }
                else
                {
                    Leader = null;
                }
            }
        }

        public void AlertSquad(Vector3 location)
        {
            TargetLocation = location;
            // Notify all members (usually via Blackboard update or event)
        }
    }
}
