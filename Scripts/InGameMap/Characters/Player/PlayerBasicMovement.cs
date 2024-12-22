using Godot;
using System;

namespace ZombieWorldWalkDemo.Scripts.InGameMap.Characters.Player
{
    /// <summary>
    /// <remarks>
    /// 此脚本已被弃用。代替它的脚本类为 BasicMovementManager。
    /// </remarks>
    /// </summary>
    [Obsolete]
    public partial class PlayerBasicMovement : CharacterBody3D
    {
        //debug用
        DebugDrawSystem DebugDrawSystem;

        //模拟响应键盘按压的Input轴的灵敏度
        float inputAxisSensitivity = 2.5f;
        //模拟响应键盘按压的Input轴的各个分量，X是左右；Z是前后
        float inputAxisX;
        float inputAxisZ;
        //模拟响应键盘按压的Input轴向量
        Vector2 inputAxis;
        //inputDir既包含了方向也包含了具体的长度值，它并不是归一的
        Vector2 inputDir;

        [Export]
        public CollisionShape3D collision;//CharacterBody的碰撞体
                                          //这里类型为dynamic，是因为在collision.Shape()调用前编译器是不知道Shape()返回的具体派生类是什么，而在C#里调用基类不存在的但存在于其派生类的方法或属性等是不允许的（基类缺少相关定义当然不能调用）,所以使用dynamic定义对象让编译器假设它确实有那么个成员可以用--当然这会停用其IntelliSense :)
        dynamic collisionShape;//碰撞体的具体形状
        [Export]
        public RayCast3D topCast;//用于蹲起检查
        [Export]
        public Node3D headPivot;//搭载玩家head的枢纽，用于响应玩家蹲起和玩家侧身的实现
        [Export]
        public Node3D head;//搭载玩家Camera的节点
        [Export]
        StairDetector stairDetector;

        [ExportCategory("鼠标控制相关")]
        [Export]
        public float mouseSensitivity = 5f;

        [ExportCategory("移动相关")]
        [Export]
        private float currentSpeedWhenMove = 0f;//玩家目前移动时的速度期望值
        [Export]
        public float walkingSpeed = 2.5f;//正常前进速度
        [Export]
        public float ambleSpeed = 1.25f;//慢步速度
        [Export]
        public float speedOfSwithToAmble = 3f;
        [Export]
        public float sprintingSpeed = 5.5f;//奔跑速度
        [Export]
        public float transitionToSprintSpeed = 1.7f;
        [Export]
        public float transitionSprintToWalkSpeed = 5f;//从奔跑速度到正常前进速度的切换速度
        [Export]
        public float transitionSprintToSquatSpeed = 3f;//从奔跑速度到蹲下时的移动速度的切换速度
        [Export]
        public bool IsSquat = false;//玩家是否蹲下了
        [Export]
        public float squattingSpeed = 1.25f;//蹲下时的移动速度

        [Export]
        public float squatUpAndDownSpeed = 2f;//蹲起速度

        [Export]
        public float stoppingSpeedOnFloor = 7f;

        [Export]
        public float jumpVelocity = 5.4f;

        [ExportCategory("蹲起相关")]
        [Export]
        public float collisionStandingHeight = 1.8f;
        [Export]
        public float cameraStandingHeight = 1.7f;
        [Export]
        public float collisionSquattingHeight = 1.2f;
        [Export]
        public float cameraSquattingHeight = 1.1f;
        [Export]
        private float cameraSquatHeightDifference;//相机蹲起高度差（负数）


        Vector3 velocity;//用来在_PhysicsProcess中处理的速度值

        /// <summary>
        /// -用来将键盘的WASD按键输入转换成模拟的2D轴输入，而不是生硬的数值切换；返回一个Vector2，最大模长为1。
        /// <para>（内部使用我自己设置的动作映射，所以WASD指的是"movement_left"、"movement_right"、"movement_forward"、"movement_backward"。）</para>
        /// </summary>
        /// <param name="delta"></param>
        /// <returns>由WASD按键控制的Vector2。</returns>
        public Vector2 GetInputAxis(float sensitivity, double delta)
        {
            //处理模拟的InputX轴
            //若无输入，
            if (!Input.IsActionPressed("movement_left") && !Input.IsActionPressed("movement_right"))
            {
                //则归零
                if (inputAxisX < 0)
                {
                    inputAxisX += (float)delta * sensitivity;
                    inputAxisX = Mathf.Clamp(inputAxisX, -1, 0);
                }
                else if (inputAxisX > 0)
                {
                    inputAxisX -= (float)delta * sensitivity;
                    inputAxisX = Mathf.Clamp(inputAxisX, 0, 1);
                }
            }//若同时输入，
            else if (Input.IsActionPressed("movement_left") && Input.IsActionPressed("movement_right"))
            {
                //亦归零，但是过程要加速（急停）
                if (inputAxisX < 0)
                {
                    inputAxisX += (float)delta * sensitivity * 2;
                    inputAxisX = Mathf.Clamp(inputAxisX, -1, 0);
                }
                else if (inputAxisX > 0)
                {
                    inputAxisX -= (float)delta * sensitivity * 2;
                    inputAxisX = Mathf.Clamp(inputAxisX, 0, 1);
                }

            }
            else//若有输入
            {
                //left则递减至-1
                if (Input.IsActionPressed("movement_left"))
                {
                    inputAxisX -= (float)delta * sensitivity;
                    inputAxisX = Mathf.Clamp(inputAxisX, -1, 0);
                }//right则递加至1
                else if (Input.IsActionPressed("movement_right"))
                {
                    inputAxisX += (float)delta * sensitivity;
                    inputAxisX = Mathf.Clamp(inputAxisX, 0, 1);
                }
            }

            //处理模拟的InputZ轴
            //若无输入
            if (!Input.IsActionPressed("movement_forward") && !Input.IsActionPressed("movement_backward"))
            {
                //则归零
                if (inputAxisZ < 0)
                {
                    inputAxisZ += (float)delta * sensitivity;
                    inputAxisZ = Mathf.Clamp(inputAxisZ, -1, 0);
                }
                else if (inputAxisZ > 0)
                {
                    inputAxisZ -= (float)delta * sensitivity;
                    inputAxisZ = Mathf.Clamp(inputAxisZ, 0, 1);
                }
            }//若同时输入
            else if (Input.IsActionPressed("movement_forward") && Input.IsActionPressed("movement_backward"))
            {
                //亦归零，但是过程要加速（急停）
                if (inputAxisZ < 0)
                {
                    inputAxisZ += (float)delta * sensitivity * 2;
                    inputAxisZ = Mathf.Clamp(inputAxisZ, -1, 0);
                }
                else if (inputAxisZ > 0)
                {
                    inputAxisZ -= (float)delta * sensitivity * 2;
                    inputAxisZ = Mathf.Clamp(inputAxisZ, 0, 1);
                }
            }
            else//若有输入
            {
                //movement_forward则递增至1
                if (Input.IsActionPressed("movement_forward"))
                {
                    inputAxisZ += (float)delta * sensitivity;
                    inputAxisZ = Mathf.Clamp(inputAxisZ, 0, 1);
                }//backward则递减至-1
                else if (Input.IsActionPressed("movement_backward"))
                {
                    inputAxisZ -= (float)delta * sensitivity;
                    inputAxisZ = Mathf.Clamp(inputAxisZ, -1, 0);
                }
            }

            inputAxis = new Vector2(inputAxisX, inputAxisZ);
            inputAxis = inputAxis.LimitLength(1);
            //GD.Print("inputAxis=",inputAxis);
            return inputAxis;

        }

        /// <summary>
        /// 处理角色的蹲起行为
        /// </summary>
        private void HandleSquat(double delta)
        {
            //按一次蹲起切换一次IsSquat的值
            //不允许空中蹲起，因为蹲跳没有用:(
            if (Input.IsActionJustPressed("movement_squat") && IsOnFloor())
            {
                //蹲下
                if (IsSquat == false)
                {
                    IsSquat = true;
                }
                else if (!topCast.IsColliding())
                {
                    IsSquat = false;
                }

            }

            //如果玩家处于蹲下状态，
            if (IsSquat)
            {
                //碰撞体修改部分
                //先检查碰撞体高度是否等于蹲下高度，等于就什么也不做
                if (collisionShape.Height == collisionSquattingHeight)
                {
                    return;
                }
                //否则再检查碰撞体高度是否低于蹲下高度，低于的就把碰撞体高度设为蹲下高度，然后返回
                else if (collisionShape.Height < collisionSquattingHeight)
                {
                    collisionShape.Height = collisionSquattingHeight;
                    return;
                }
                //而高于蹲下高度则说明还没蹲完，按照蹲起速度减碰撞体的高度
                else if (collisionShape.Height > collisionSquattingHeight)
                {
                    collisionShape.Height -= squatUpAndDownSpeed * (float)delta;
                }

                //碰撞体Position的修改部分
                //碰撞体的Position很重要，由于碰撞体原点位于中央高度而不是最低点，故每当修改碰撞体的高度都要修改其相对与CharacterBody的高度位置，使两者位置点（Position）重合，否则会出现摄像机等部件的高度偏差问题（根本原因是CharacterBody的位置受碰撞体的影响，带来各种各样的错位问题）
                //如果碰撞体原点的高度等于玩家蹲下时它应该所在的高度，则什么也不做
                if (collision.Position.Y == collisionSquattingHeight / 2)
                {
                    return;
                }
                //小于说明减过头了，直接改为目标高度后返回
                else if (collision.Position.Y < collisionSquattingHeight / 2)
                {
                    Vector3 position = collision.Position;
                    position.Y = collisionSquattingHeight / 2;
                    collision.Position = position;
                    return;
                }
                //否则，大于应到高度就减高度差/2
                else if (collision.Position.Y > collisionSquattingHeight / 2)
                {
                    Vector3 position = collision.Position;
                    position.Y -= squatUpAndDownSpeed * (float)delta / 2;
                    collision.Position = position;
                }

                //headPivot修改部分
                //蹲下时headPivot高度设置（对应玩家摄像机）
                if (headPivot.Position.Y == cameraSquatHeightDifference)
                {
                    return;
                }
                //大于需蹲下高度说明没蹲完，减
                else if (headPivot.Position.Y > cameraSquatHeightDifference)
                {
                    //C#里Node3/2D.Position的各个分量不是变量而是属性，要修改就要声明个缓冲用的本地变量
                    Vector3 position;
                    position = Vector3.Zero;
                    position.Y -= squatUpAndDownSpeed * (float)delta;
                    headPivot.Position += position;
                    return;
                }
                //小于蹲下高度说明减过头了
                else if (headPivot.Position.Y < cameraSquatHeightDifference)
                {
                    Vector3 position;
                    position = new Vector3(headPivot.Position.X, cameraSquatHeightDifference, headPivot.Position.Z);
                    headPivot.Position = position;
                }

            }
            //如果玩家处于站立状态（IsSquat=false）
            else
            {
                //碰撞体修改部分
                //则检查碰撞体高度是否等于站立高度，等于就什么也不做
                if (collisionShape.Height == collisionStandingHeight)
                {
                    return;
                }
                //大于站立高度则设为站立高度然后返回
                else if (collisionShape.Height > collisionStandingHeight)
                {
                    collisionShape.Height = collisionStandingHeight;
                }
                //低于则说明没站起来完，加碰撞体高度
                else if (collisionShape.Height < collisionStandingHeight)
                {
                    collisionShape.Height += squatUpAndDownSpeed * (float)delta;
                }


                //碰撞体Position的修改部分
                //如果原点与玩家站立时应到的高度一致则什么也不做
                if (collision.Position.Y == collisionStandingHeight / 2)
                {
                    return;
                }
                //大于说明加过头了，直接改为目标高度后返回
                else if (collision.Position.Y > collisionStandingHeight / 2)
                {
                    Vector3 position = collision.Position;
                    position.Y = collisionStandingHeight / 2;
                    collision.Position = position;
                    return;
                }
                //否则，小于应到高度就加高度差/2
                else if (collision.Position.Y < collisionStandingHeight / 2)
                {
                    Vector3 position = collision.Position;
                    position.Y += squatUpAndDownSpeed * (float)delta / 2;
                    collision.Position = position;
                }


                //headPivot修改部分
                //站立时headPivot高度设置（对应玩家摄像机）
                if (headPivot.Position.Y == 0f)
                {
                    return;
                }
                //大于站立高度说明加过头了，headPivot高度设置为0后返回
                else if (headPivot.Position.Y > 0f)
                {
                    Vector3 position;
                    position = new Vector3(headPivot.Position.X, 0f, headPivot.Position.Z);
                    headPivot.Position = position;
                }
                //小于站立高度说明还没站起来完，加
                else if (headPivot.Position.Y < 0)
                {
                    Vector3 position;
                    position = Vector3.Zero;
                    position.Y += squatUpAndDownSpeed * (float)delta;
                    headPivot.Position += position;
                    return;

                }

            }

        }

        /// <summary>
        /// 玩家重力处理。
        /// 返回处理后的速度值
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        private Vector3 HandleGravity(Vector3 velocity, double delta)
        {
            // Add the gravity.
            if (!IsOnFloor())
            {
                //GetGravity()返回表示重力的Vector3，默认是(0，-9.8，0)。
                velocity += GetGravity() * (float)delta * 1.5f;
            }
            return velocity;
        }

        /// <summary>
        /// 玩家跳跃处理。
        /// 返回处理后的速度值
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        private Vector3 HandleJump(Vector3 velocity)
        {
            if (Input.IsActionJustPressed("movement_jump"))
            {
                //有 或stairDetector.wasSnappedToStairsLastFrame 是因为有时候玩家下楼梯时IsOnFloor可能为false
                if ((IsOnFloor() || stairDetector.wasSnappedToStairsLastFrame) && IsSquat && !topCast.IsColliding())
                {
                    IsSquat = false;
                }
                else if (IsOnFloor() || stairDetector.wasSnappedToStairsLastFrame)
                {
                    velocity.Y = jumpVelocity;
                }
            }


            return velocity;
        }

        /// <summary>
        /// Character速度换挡判断
        /// </summary>
        /// <param name="delta"></param>
        private void SpeedShiftJudge(double delta)
        {
            //根据是否奔跑或蹲下或步行来修改Character的速度挡位
            //如果玩家是蹲下的,
            if (IsSquat == true)
            {
                if (currentSpeedWhenMove != squattingSpeed)
                {
                    //则递减至目标速度
                    currentSpeedWhenMove -= transitionSprintToSquatSpeed * (float)delta;
                }

                if (currentSpeedWhenMove < squattingSpeed)
                {
                    currentSpeedWhenMove = squattingSpeed;
                }
                return;
            }
            else
            {
                //如果按下了奔跑键且按下了前进键则速度修改至奔跑
                if (Input.IsActionPressed("movement_sprint") && Input.IsActionPressed("movement_forward"))
                {
                    //根据切换速度将currentSpeedWhenMove切换到奔跑的速度
                    currentSpeedWhenMove += transitionToSprintSpeed * (float)delta;
                    if (currentSpeedWhenMove > sprintingSpeed)
                    {
                        currentSpeedWhenMove = sprintingSpeed;
                    }
                }
                else if (currentSpeedWhenMove > walkingSpeed)
                {
                    //如果没有奔跑但是速度大于奔跑速度的,根据切换速度将currentSpeedWhenMove切换到正常移动的速度
                    currentSpeedWhenMove -= transitionSprintToWalkSpeed * (float)delta;
                    if (currentSpeedWhenMove < walkingSpeed)
                    {
                        currentSpeedWhenMove = walkingSpeed;
                    }
                }
                else if (Input.IsActionPressed("movement_amble"))
                {
                    currentSpeedWhenMove -= speedOfSwithToAmble * (float)delta;
                    if (currentSpeedWhenMove < ambleSpeed)
                    {
                        currentSpeedWhenMove = ambleSpeed;
                    }
                }
                else
                {
                    currentSpeedWhenMove = walkingSpeed;
                }

            }



        }

        public override void _Ready()
        {
            //锁🔒鼠标
            Input.MouseMode = Input.MouseModeEnum.Captured;
            //获取相关节点
            //collision = GetNode<CollisionShape3D>("playerCollision");
            //topCast = GetNode<RayCast3D>("topCast");
            //head = GetNode<Node3D>("head");
            //获取Character碰撞体的形状
            collisionShape = collision.Shape;
            //初始化蹲起用的数值
            collisionStandingHeight = collisionShape.Height;
            cameraStandingHeight = head.Position.Y;
            cameraSquatHeightDifference = -(cameraStandingHeight - cameraSquattingHeight);

            //Debug用
            DebugDrawSystem = GetNode<DebugDrawSystem>("/root/DebugDrawSystem");
        }

        //这部分处理head节点的旋转来代表第一人称视角相机的基本控制
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                //左右旋转的是玩家的body
                this.RotateY(Mathf.DegToRad(-mouseMotion.Relative.X / 100 * mouseSensitivity));
                //上下旋转的是玩家的head
                head.RotateX(Mathf.DegToRad(-mouseMotion.Relative.Y / 100 * mouseSensitivity));
                //Leo：上下限制角度不能为90度，会有奇怪的卡顿和粘滞；
                //Y轴和Z轴不能设为0，以后应用第一人称动画可能会用到
                head.Rotation = new Vector3(Mathf.Clamp(head.Rotation.X, -Mathf.DegToRad(87), Mathf.DegToRad(85)), head.Rotation.Y, head.Rotation.Z);
            }
        }

        public override void _PhysicsProcess(double delta)
        {

            inputDir = GetInputAxis(inputAxisSensitivity, delta);

            velocity = Velocity;

            HandleSquat(delta);



            SpeedShiftJudge(delta);

            velocity = HandleGravity(velocity, delta);

            velocity = HandleJump(velocity);

            // Get the input direction and handle the movement/deceleration.
            //Transform.Basis是3x3矩阵，用于将矢量从世界坐标系转换为CharacterBody3D的局部坐标系
            Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, -inputDir.Y));

            //如果有角色移动的输入，修改Character的Velocity
            if (Input.IsActionPressed("movement_forward") | Input.IsActionPressed("movement_backward") | Input.IsActionPressed("movement_left") | Input.IsActionPressed("movement_right"))
            {
                velocity.X = direction.X * currentSpeedWhenMove;
                velocity.Z = direction.Z * currentSpeedWhenMove;
            }
            else
            {
                DecreaseSpeedUntilStop(delta);
            }

            //GD.Print("CharacterBody3D's Velocity:",velocity.Length());
            Velocity = velocity;
            //GD.Print("y轴速度：", Velocity.Y);

            if (!stairDetector.SnapUpStairsCheck(delta))
            {
                MoveAndSlide();

                stairDetector.SnapDownToStairsCheck();//在MoveAndSlide()后调用
            }

        }

        /// <summary>
        /// 当玩家输入为零的时候调用，递减速度值至0。
        /// 返回处理过的velocity值
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        private Vector3 DecreaseSpeedUntilStop(double delta)
        {
            //若角色站在地上
            if (IsOnFloor())
            {
                //且当Character的速度不为0时,减速度的相反向量直至为零向量
                if (velocity.X != 0 | velocity.Z != 0)
                {
                    var oldVelX = velocity.X;
                    var oldVelZ = velocity.Z;
                    velocity += -velocity.Normalized() * stoppingSpeedOnFloor * (float)delta;
                    var newVelX = velocity.X;
                    var newVelZ = velocity.Z;

                    if (oldVelX > 0 && newVelX < 0)
                    {
                        velocity.X = 0;
                    }
                    if (oldVelX < 0 && newVelX > 0)
                    {
                        velocity.X = 0;
                    }

                    if (oldVelZ > 0 && newVelZ < 0)
                    {
                        velocity.Z = 0;
                    }
                    if (oldVelZ < 0 && newVelZ > 0)
                    {
                        velocity.Z = 0;
                    }
                }

            }

            //若角色在空中,不对速度进行任何处理
            return velocity;


        }
    }
}
