using Godot;
using System;

namespace ZombieWorldWalkDemo.Scripts.InGameMap
{
    //正式游戏场景内使用的 Debug 绘制系统，除了主菜单场景，每一个游戏地图场景都（需要）会有一个该节点来管理地图内的 Debug 绘制操作.
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
}
