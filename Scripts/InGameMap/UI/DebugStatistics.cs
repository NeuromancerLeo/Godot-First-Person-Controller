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
        Label inputVelocityMultiplier;
        [Export]
        Label velocity;
        
        [Export]
        Label health;
        [Export]
        Label healthRecoverySpeed;
        [Export]
        Label endurance;
        [Export]
        Label enduranceRecoverySpeed;
        [Export]
        Label hunger;
        [Export]
        Label hungerRecoverySpeed;
        [Export]
        Label defense;
        [Export]
        Label carryWeight;
        [Export]
        Label maxCarryWeight;

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
                inputVelocityMultiplier.Text = "inputVelocityMultiplier: " + _playerStats.InputVelocityMultiplier;
                velocity.Text = "velocity: " + _playerBody.Velocity.Length().ToString("F3") + " m/s";
                health.Text = "health: " + _playerStats.Health.ToString("F3") + " / " + _playerStats.MaxHealth;
                healthRecoverySpeed.Text = "healthRecoverySpeed: " + _playerStats.HealthRecoverySpeed.ToString("F3") + "/s";
                endurance.Text = "endurance: " + _playerStats.Endurance.ToString("F3") + " / " + _playerStats.MaxEndurance;
                enduranceRecoverySpeed.Text = "enduranceRecoverySpeed: " + _playerStats.EnduranceRecoverySpeed.ToString("F3") + "/s";
                hunger.Text = "hunger: " + _playerStats.Hunger.ToString("F3") + " / " + _playerStats.MaxHunger;
                hungerRecoverySpeed.Text = "hungerRecoverySpeed: " + _playerStats.HungerRecoverySpeed.ToString("F3") + "/s";
                defense.Text = "defense: " + _playerStats.Defense.ToString("F3") + " / " + _playerStats.MaxDefense;
                carryWeight.Text = "carryWeight(kg): " + _playerStats.CarryWeight.ToString("F3") + " / " + _playerStats.MaxCarryWeight.ToString("F3");
                maxCarryWeight.Text = "maxCarryWeight(kg): " + _playerStats.MaxCarryWeight.ToString("F3");
            }
        }
    }
}
