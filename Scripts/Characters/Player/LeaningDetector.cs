using Godot;
using System;

public partial class LeaningDetector : Node3D
{


    [Export]
    Node3D head;
    [Export]
    ShapeCast3D leftShapeCast;
    [Export]
    ShapeCast3D rightShapeCast;

    public bool isAllowToLeanLeft = false;
    public bool isAllowToLeanRight = false;

    Transform3D globalTransform;

    public override void _PhysicsProcess(double delta)
    {
        //跟随head的Y轴坐标，以配合蹲起
        globalTransform = this.GlobalTransform;
        globalTransform.Origin.Y = head.GlobalTransform.Origin.Y;
        this.GlobalTransform = globalTransform;
        //检查左右边的shapeCast是否与障碍物碰撞，是则设置对应的isAllowToLean为false，否则设置为true
        //左边
        if (leftShapeCast.IsColliding())
        {
            isAllowToLeanLeft = false;
            //GD.Print("isAllowToLeanLeft = false");
        }
        else
        {
            isAllowToLeanLeft = true;
        }

        //右边
        if (rightShapeCast.IsColliding())
        {
            isAllowToLeanRight = false;
            //GD.Print("isAllowToLeanRight = false");
        }
        else
        {
            isAllowToLeanRight = true;
        }
        //在PlayerHeadLeaning脚本那我们会调用isAllowtoLeanLeft/Right来看是否允许执行侧身操作
    }
}
