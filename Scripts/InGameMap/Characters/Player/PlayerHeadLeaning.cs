using Godot;
using System;
using ZombieWorldWalkDemo.Scripts.InGameMap.Characters.Player;

namespace ZombieWorldWalkDemo.Scripts.InGameMap.Characters.Player
{
    /// <summary>
    /// 该脚本内容为玩家侧身的实现
    /// </summary>
    public partial class PlayerHeadLeaning : Node3D
    {
        public bool isTryToLean;
        bool isTryToLeanLeft;
        bool isTryToLeanRight;

        [Export]
        LeaningDetector detector;

        [Export]
        float leaningAngle = 12f;//侧身角度
        [Export]
        float leaningSpeed = 30f;//侧身速度


        public override void _PhysicsProcess(double delta)
        {
            //如果玩家在按侧身键，就设置对应的isTryToLean为true，否则为false
            if (Input.IsActionPressed("movement_lean_left"))
            {
                isTryToLeanLeft = true;
            }
            else
            {
                isTryToLeanLeft = false;
            }
            if (Input.IsActionPressed("movement_lean_right"))
            {
                isTryToLeanRight = true;
            }
            else
            {
                isTryToLeanRight = false;
            }
            //如果左右侧身同时按住或左右侧身都没按住
            if ((Input.IsActionPressed("movement_lean_left") && Input.IsActionPressed("movement_lean_right")) || (!Input.IsActionPressed("movement_lean_left") && !Input.IsActionPressed("movement_lean_right")))
            {
                isTryToLean = false;
                isTryToLeanLeft = false;
                isTryToLeanRight = false;
            }
            else
            {
                isTryToLean = true;
            }

            HandleLeaning(delta);

            //GD.Print(this.RotationDegrees.Z);
            //GD.Print("IsActionPressed(\"movement_lean_left\") == ", Input.IsActionPressed("movement_lean_left"));
            //GD.Print("IsActionPressed(\"movement_lean_right\") == ", Input.IsActionPressed("movement_lean_right"));
            //GD.Print("isLeaning == ",isLeaning);
        }

        /// <summary>
        /// 处理玩家侧身操作
        /// <para>Leo: 这里只有无尽的if else......</para>
        /// </summary>
        /// <param name="delta"></param>
        public void HandleLeaning(double delta)
        {
            //如果没有按任何侧身键，检查headPivot的Z轴旋转是否已恢复到正常直立
            if (isTryToLean == false)
            {
                //若headPivot的Z轴旋转值为0
                if (this.RotationDegrees.Z == 0f)
                {
                    //则直接返回
                    return;
                }
                //不为0则需要根据侧身速度恢复旋转值至0
                //旋转值大于0时
                else if (this.RotationDegrees.Z > 0)
                {
                    //旋转负值
                    this.RotateZ(-Mathf.DegToRad(leaningSpeed * (float)delta));
                    //在Z轴上进行钳制，最小只能减到0
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, 0, Mathf.DegToRad(leaningAngle)));
                }
                //旋转值小于0时
                else if (this.RotationDegrees.Z < 0)
                {
                    //旋负正值
                    this.RotateZ(Mathf.DegToRad(leaningSpeed * (float)delta));
                    //在Z轴上进行钳制，最大只能加到0
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, -Mathf.DegToRad(leaningAngle), 0));
                }
            }
            //按下了侧身键，但不被允许相应的侧身时，亦执行恢复操作
            //按下不被允许的左，
            else if (isTryToLeanLeft && !detector.isAllowToLeanLeft)
            {
                //当前旋转值若等于0直接返回即可，不处理
                if (this.RotationDegrees.Z == 0f)
                {
                    return;
                }
                //当前旋转值大于0（左）时
                else if (this.RotationDegrees.Z > 0)
                {
                    //旋转负值以至0
                    this.RotateZ(-Mathf.DegToRad(leaningSpeed * 1.5f * (float)delta));
                    //在Z轴上进行钳制，最小只能减到0
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, 0, Mathf.DegToRad(leaningAngle)));
                }
                //旋转值小于0（右）时
                //（这里，如果玩家是在按下右侧身时按下了不被允许的左侧身，那么恢复旋转值也是符合道理的）
                else if (this.RotationDegrees.Z < 0)
                {
                    //可以恢复旋转值至0
                    this.RotateZ(Mathf.DegToRad(leaningSpeed * 1.5f * (float)delta));
                    //在Z轴上进行钳制，最大只能加到0
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, -Mathf.DegToRad(leaningAngle), 0));
                }
            }
            //按下不被允许的右，也执行恢复操作
            else if (isTryToLeanRight && !detector.isAllowToLeanRight)
            {
                //当前旋转值若等于0直接返回即可，不处理
                if (this.RotationDegrees.Z == 0f)
                {
                    return;
                }
                //当前旋转值大于0（左）时
                else if (this.RotationDegrees.Z > 0)
                {
                    //可以旋转负值以至0
                    this.RotateZ(-Mathf.DegToRad(leaningSpeed * 1.5f * (float)delta));
                    //在Z轴上进行钳制，最小只能减到0
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, 0, Mathf.DegToRad(leaningAngle)));
                }
                //旋转值小于0（右）时
                else if (this.RotationDegrees.Z < 0)
                {
                    //旋转正值以至0
                    this.RotateZ(Mathf.DegToRad(leaningSpeed * 1.5f * (float)delta));
                    //在Z轴上进行钳制，最大只能加到0
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, -Mathf.DegToRad(leaningAngle), 0));
                }
            }
            //如果按下侧身键时没有按下不被允许的侧身键
            //那么就会来到下面的else if
            //
            //旋转挂载着这个脚本的headPivot节点，以实现侧身的效果
            //
            //处理左侧身
            else if (isTryToLeanLeft && detector.isAllowToLeanLeft)
            {
                //GD.Print("isLeaningLeft");
                //如果目前旋转值已经符合倾斜角度，直接返回
                if (this.RotationDegrees.Z == leaningAngle)
                {
                    return;
                }
                //若小于倾斜角度
                else if (this.RotationDegrees.Z < leaningAngle)
                {

                    //按侧身速度旋转headPivot
                    this.RotateZ(Mathf.DegToRad(leaningSpeed * (float)delta));
                    //在Z轴上进行钳制，最大只能加到leaningAngle
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, -Mathf.DegToRad(leaningAngle + 1f), Mathf.DegToRad(leaningAngle)));
                }
            }
            //处理右侧身
            else if (isTryToLeanRight && detector.isAllowToLeanRight)
            {
                //GD.Print("isLeaningRight");
                //如果目前旋转值已经符合倾斜角度，直接返回
                if (this.RotationDegrees.Z == -leaningAngle)
                {
                    return;
                }
                //若大于倾斜角度
                else if (this.RotationDegrees.Z > -leaningAngle)
                {
                    //按侧身速度旋转headPivot
                    this.RotateZ(-Mathf.DegToRad(leaningSpeed * (float)delta));
                    //在Z轴上进行钳制，最小只能减到-leaningAngle
                    this.Rotation = new Vector3(this.Rotation.X, this.Rotation.Y, Mathf.Clamp(this.Rotation.Z, -Mathf.DegToRad(leaningAngle), Mathf.DegToRad(leaningAngle + 1f)));
                }
            }


        }
    }
}
