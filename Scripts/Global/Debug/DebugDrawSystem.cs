using Godot;
using System;

public partial class DebugDrawSystem : Node
{

    [Export]
    PackedScene _cubeShape;

    /// <summary>
    /// 在给定的位置实例化一个debug用的cube
    /// </summary>
    /// <param name="targetPosistion"></param>
    public void DrawCube(Vector3 targetPosistion)
    {
        //GD.Print("Debug3D.DrawCube被调用");
        dynamic _cube = _cubeShape.Instantiate();
        _cube.Position = targetPosistion;
        AddChild(_cube);
    }
}
