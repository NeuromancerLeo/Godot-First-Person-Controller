using Godot;
using System;
using ZombieWorldWalkDemo.Scripts.Global;

namespace ZombieWorldWalkDemo.Scripts.InGameMap.UI
{
    public partial class DebugStatistics : Control
    {
        [ExportCategory("玩家数据相关")]
        PlayerStatisticsManager _playerStats;
        CharacterBody3D _playerBody;
        [Export]
        Label velocity;
        [Export]
        Label health;
        [Export]
        Label endurance;
        [Export]
        Label hunger;
        [Export]
        Label defense;
        [Export]
        Label carryWeight;

        public override void _Ready()
        {
            _playerStats = GetNode<PlayerStatisticsManager>("/root/PlayerStatisticsManager");
            _playerBody = GetNode<CharacterBody3D>("/root/World/Player");
        }

        public override void _Process(double delta)
        {
            if (!Visible)
            {
                return;
            }
            else
            {
                velocity.Text = "velocity: " + _playerBody.Velocity.Length() + " m/s";
                health.Text = "health: " + _playerStats.Health.ToString() + " / " + _playerStats.MaxHealth;
                endurance.Text = "endurance: " + _playerStats.Endurance.ToString() + " / " + _playerStats.MaxEndurance;
                hunger.Text = "hunger: " + _playerStats.Hunger.ToString() + " / " + _playerStats.MaxHunger;
                defense.Text = "defense: " + _playerStats.Defense.ToString() + " / " + _playerStats.MaxDefense;
                carryWeight.Text = "carryWeight: " + _playerStats.CarryWeight.ToString() + " kg/" + _playerStats.MaxCarryWeight + " kg";
            }
        }
    }
}
