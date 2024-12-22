using Godot;
using System;

namespace ZombieWorldWalkDemo.Scripts.InGameMap.Props
{
    public partial class DebugShapeCube : Node3D
    {
        float timer;
        float duration = 5f;

        public override void _Process(double delta)
        {
            if (timer < duration)
            {
                timer += (float)delta;
            }
            if (timer > duration)
            {
                this.QueueFree();
            }
        }
    }
}
