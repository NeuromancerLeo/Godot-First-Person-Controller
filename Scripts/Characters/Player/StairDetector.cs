using Godot;
using System;

/// <summary>
/// 台阶检测器，用于给玩家控制的 CharacterBody 添加上下阶梯的功能
/// </summary>
public partial class StairDetector : Node3D
{
    //debug用
    //DebugDrawSystem DebugDrawSystem;

    [Export]
    CharacterBody3D playerBody;

    [ExportCategory("检测用射线")]
    [Export]
    RayCast3D stairsAheadRayCast;//用来检测阶梯是否陡峭的射线，为垂直向下，长度要比最大上阶高度要长一点
    [Export]
    RayCast3D stairsBelowRayCast;
    
    [ExportCategory("相关参数")]
    //最大上阶高度
    [Export]
    float maxStepHeight = 0.3f;
    //最大上阶角度（弧度）
    [Export]
    float maxStepAngle = Mathf.Pi/180f * 5f;

    public bool wasSnappedToStairsLastFrame = false;

    /// <summary>
    /// 当 playerBody.IsOnFloor 为true时，从 Engine.GetPhysicsFrames 那持续记录的物理帧的编号
    /// </summary>
    float lastFrameWasOnFloor = float.NegativeInfinity;

    /// <summary>
    /// 通过检查输入的法线来确认是否该法线所属的碰撞面是否过于陡峭
    ///<para>根据最大上阶角度来决定</para>
    /// </summary>
    /// <param name="normal"></param>
    /// <returns></returns>
    public bool IsSurfaceTooSteep(Vector3 normal)
    {
        //注意 AngleTo 方法返回的是弧度值
        return normal.AngleTo(Vector3.Up) > maxStepAngle;

    }

    public bool RunPlayerBodyTestMotion(Transform3D from, Vector3 motion, PhysicsTestMotionResult3D result)
    {
        //Leo：待了解
        //Leo：待了解
        //result 包含了各种各样的返回信息
        if(result == null)
        {
            result = new PhysicsTestMotionResult3D();
        }

        //设置 TestMotion 所需参数，From 为测试起点，motion 为方向矢量
        var _parameters = new PhysicsTestMotionParameters3D();
        _parameters.From = from;
        _parameters.Motion = motion;

        //用 playerBody 进行运动测试
        return PhysicsServer3D.BodyTestMotion(playerBody.GetRid(), _parameters, result);
        
    }

    /// <summary>
    /// 执行玩家吸附至台阶下表面的检查，如果符合条件会将玩家吸附至台阶下表面，并设置 <see cref="wasSnappedToStairsLastFrame"/> 为 <see cref="true"/>.
    /// <para>否则 <see cref="wasSnappedToStairsLastFrame"/> 会被设置为 <see cref="false"/>，然后什么也不干.</para>
    /// </summary>
    public void SnapDownToStairsCheck()
    {
        bool _didSnap = false;
        //检查玩家下方是否有可供吸附的较为平坦的平台
        bool _isFloorBelow = (stairsBelowRayCast.IsColliding() && !IsSurfaceTooSteep(stairsBelowRayCast.GetCollisionNormal()));
        //检查在上一帧时玩家是否 IsOnfloor：
        //用目前的物理帧编号数减去 lastFrameWasOnFloor，判断差值是否为1即可
        bool _wasOnFloorLastFrame = (Engine.GetPhysicsFrames() - lastFrameWasOnFloor == 1);

        //下面是执行吸附的逻辑
        //如果玩家在空中且在下落时，继续检查，当玩家上一帧在地面或上一帧吸附到阶梯时，并且玩家下方有可供吸附的平台时
        //执行吸附
        if (!playerBody.IsOnFloor() && playerBody.Velocity.Y <= 0 && (_wasOnFloorLastFrame || wasSnappedToStairsLastFrame) && _isFloorBelow)
        {  
            PhysicsTestMotionResult3D _bodyTestMotionResult = new PhysicsTestMotionResult3D();

            //根据最大阶梯高度来向下模拟移动玩家的 playerBody，如果有碰撞便返回true
            if (RunPlayerBodyTestMotion(playerBody.GlobalTransform, new Vector3(0,-maxStepHeight,0), _bodyTestMotionResult))
            {
                var _translateY = _bodyTestMotionResult.GetTravel().Y;
                playerBody.Position += new Vector3(0, _translateY, 0);
                playerBody.ApplyFloorSnap();
                _didSnap = true;
            }        
        
        }

        wasSnappedToStairsLastFrame = _didSnap;
    
    }

    /// <summary>
    /// 执行玩家上阶梯的检查，符合条件的就会将玩家抬上阶梯并返回 true
    /// <para>不符合检查条件时，就会什么也不做并返回 false</para>
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public bool SnapUpStairsCheck(double delta)
    {
        //如果这帧玩家在空中，且上一帧没有在进行吸附阶梯，则不处理，返回false
        if (!playerBody.IsOnFloor() && !wasSnappedToStairsLastFrame)
        {
            //GD.Print("玩家在空中，且上一帧没有在进行吸附阶梯");
            return false;
        }
        else
        {
            //这是玩家在水平方向上的移动矢量 * 物理帧 delta（即该帧的移动量） * 系数，
            //用来定义玩家前上方的矢量，下面会根据这个前上方矢量的点位置
            //测试将 PlayerBody 向下打看会不会碰撞到台阶之类的平台
            Vector3 _expectedMoveMotion = playerBody.Velocity * new Vector3(1, 0, 1) * (float)delta * 1.6f;

            //这是表示将玩家按照 该帧移动量+大小为最大上阶高度的 Vector.Up 的矢量 移动后的结果 Transform3D
            Transform3D _stepPosWithClearance = playerBody.GlobalTransform.Translated(_expectedMoveMotion + new Vector3(0, maxStepHeight, 0));

            //碰撞结果信息的容器
            PhysicsTestMotionResult3D _downCheckResult = new PhysicsTestMotionResult3D();

            //开始运动测试，起点为上阶后的大致位置，然后向下（Vector3(0,-maxStepHeight,0)）运动看是否有碰撞,无碰撞便返回 false
            //Leo:这是每物理帧都会执行的，不过对性能应该没多大影响，大概。
            if (RunPlayerBodyTestMotion(_stepPosWithClearance, new Vector3(0, -maxStepHeight, 0), _downCheckResult))
            {
                //GD.Print("前上向下打检测到了碰撞");
                //DebugDrawSystem.DrawCube(_downCheckResult.GetCollisionPoint());

                //如果碰撞到的碰撞体为 "StaticBody3D" 或 "CSGShape3D" 我们就会继续玩家上台阶的操作，否则返回 false
                if (_downCheckResult.GetCollider().IsClass("StaticBody3D") || _downCheckResult.GetCollider().IsClass("CSGShape3D"))
                {
                    //获取高度
                    //Leo:我不是很懂这个变量，但是一切照常进行所以我想没什么大问题 XD
                    //Leo:如果你想获取台阶高度请使用 (_downCheckResult.GetCollisionPoint() - player.GlobalPosition).Y）
                    float _steppingHeight = ((_stepPosWithClearance.Origin + _downCheckResult.GetTravel()) - playerBody.GlobalPosition).Y;

                    //检查上阶高度是否符合我们的 maxStepHeight（_steppingHeight 不能<= 0.01f 是因为防止非预期的抖动）,不符合就返回 false
                    if (_steppingHeight < maxStepHeight && _steppingHeight > 0.01f && (_downCheckResult.GetCollisionPoint() - playerBody.GlobalPosition).Y < maxStepHeight)
                    {
                        //符合就继续
                        //在把玩家传送到阶梯上前我们最后需要投射一个射线用来检测阶梯是否陡峭
                        //设置 stairsAheadRayCast 的起点位置
                        stairsAheadRayCast.GlobalPosition = _downCheckResult.GetCollisionPoint() + new Vector3(0, maxStepHeight, 0) + _expectedMoveMotion.Normalized() * 0.1f;
                        //默认情况下生成射线后要到下一帧才会工作，使用这个方法可以强制射线在当前帧执行检测
                        stairsAheadRayCast.ForceRaycastUpdate();

                        //射线检测碰撞,没检测到碰撞就返回 false
                        if (stairsAheadRayCast.IsColliding())
                        {
                            //如果阶梯的平台并不陡峭,传送玩家上阶梯,否则返回 false
                            if (IsSurfaceTooSteep(stairsAheadRayCast.GetCollisionNormal()) == false)
                            {
                                //GD.Print("IsSurfaceTooSteep为false。");
                                playerBody.GlobalPosition = _stepPosWithClearance.Origin + _downCheckResult.GetTravel();
                                playerBody.ApplyFloorSnap();
                                wasSnappedToStairsLastFrame = true;
                                return true;
                            }
                            else
                            {
                                //GD.Print("检测到平台陡峭，上不去");
                                return false;
                            }
                        }
                        else
                        {
                            //GD.Print("最后一步的stairsAheadRayCast没有检测到碰撞");
                            return false;
                        }
                    }                   
                    else
                    {

                        //GD.Print("台阶高度不符合设定，获取到的台阶高度为：", _steppingHeight);
                        //GD.Print("获取到的碰撞点 - 玩家位置 的 Y轴差值为：",(_downCheckResult.GetCollisionPoint() - player.GlobalPosition).Y);
                        //画个方块我看看这么个事
                        //DebugDrawSystem.DrawCube(_downCheckResult.GetCollisionPoint());
                        return false;
                    }
                }
                else
                {
                    //GD.Print("碰撞到的碰撞体不为 StaticBody3D 或 CSGShape3D ");
                    return false;
                }
            }
            else
            {
                //GD.Print("前上向下打没有检测到碰撞");
                return false;
            }
        }

       
    }

    public override void _Ready()
    {
        //Debug用
        //DebugDrawSystem = GetNode<DebugDrawSystem>("/root/DebugDrawSystem");
    }

    public override void _PhysicsProcess(double delta)
    {
        //当玩家的 playerBody.IsOnFloor 为true时，
        if (playerBody.IsOnFloor())
        {
            //从Engine.GetPhysicsFrames那持续记录物理帧的编号
            lastFrameWasOnFloor = Engine.GetPhysicsFrames();
        }
    }
}

