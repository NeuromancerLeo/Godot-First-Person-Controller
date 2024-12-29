using Godot;
using System;
using System.Linq;
using ZombieWorldWalkDemo.Scripts.Global;

namespace ZombieWorldWalkDemo.Scripts.InGameMap.Props
{
    public partial class RazorWire : Area3D
    {
        PlayerStatisticsManager _playerStats;

        private static bool IsPlayer(Node3D body)
        {
            return body.Name == "Player";
        }

        private void OnEnterArea(Node3D body)
        {
            if (body is CharacterBody3D)
            {
                _playerStats.ModifyInputVelocityMultiplier(PlayerStatisticsManager.ModifyType.Set, 0.5f, "RazorWire");
                _playerStats.ModifyHealthRecoverySpeed(PlayerStatisticsManager.ModifyType.Add, -5f, "RazorWire");
            }
        }

        private void OnExitArea(Node3D body)
        {
            if (body is CharacterBody3D)
            {
                _playerStats.ModifyInputVelocityMultiplier(PlayerStatisticsManager.ModifyType.Set, 1f, "RazorWire");
                _playerStats.ModifyHealthRecoverySpeed(PlayerStatisticsManager.ModifyType.Add, +5f, "RazorWire");
            }
        }

        public override void _Ready()
        {
            _playerStats = GetNode<PlayerStatisticsManager>("/root/PlayerStatisticsManager");
            this.BodyEntered += OnEnterArea;
            this.BodyExited += OnExitArea;
        }

        /*public override void _PhysicsProcess(double delta)
        {
            if (HasOverlappingBodies() && GetOverlappingBodies().Any(IsPlayer))
            {
                //GD.Print(GetOverlappingBodies().Any(IsPlayer));
            }
        }*/
    }
}
