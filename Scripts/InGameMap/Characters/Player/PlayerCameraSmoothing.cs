using Godot;
using System;

namespace ZombieWorldWalkDemo.Scripts.InGameMap.Characters.Player
{
	public partial class PlayerCameraSmoothing : Camera3D
	{
		//注：Player 的 head 在 BasicMovementManager.cs 中处理，响应鼠标输入作为自身的旋转量，代表了玩家的第一人称视角旋转。
		//该脚本的作用是将 Player 的 head.GlobalTransform 进行 物理插值 ，应用在 Player 的 Camera 上（Camera 本身不跟随父节点的变换），
		//使其在每一渲染帧中的运动变得平滑（而不是可怕的抖动）

		//target 即 head 的引用
		[Export]
		Node3D target;
		Transform3D oldTransf;
		Transform3D newTransf;

		bool isPhysicsUpdate = false;

		public override void _Ready()
		{
			//Camera 本身不跟随父节点的变换
			this.TopLevel = true;
			//head 是 playerCamera 的父级
			//target = GetNode("../head");
			//初始化
			this.GlobalTransform = target.GlobalTransform;
			oldTransf = target.GlobalTransform;
			newTransf = target.GlobalTransform;
		}

		//新值赋给旧值，然后更新新值
		private void UpdateTransform()
		{
			oldTransf = newTransf;
			newTransf = target.GlobalTransform;
		}

		public override void _Process(double delta)
		{
			if (isPhysicsUpdate)
			{
				UpdateTransform();
				isPhysicsUpdate = false;
			}

			//获取物理插值分数（详情可去 Godot 文档查阅）
			var f = Mathf.Clamp(Engine.GetPhysicsInterpolationFraction(), 0, 1);
			//根据newTransf物理插值oldTransf并应用
			this.GlobalTransform = oldTransf.InterpolateWith(newTransf, (float)f);
		}

		public override void _PhysicsProcess(double delta)
		{
			//每更新一次物理帧就更新一次 Transform,其余的时间就会一直物理插值平滑 Transform 在每渲染帧的值
			isPhysicsUpdate = true;
		}
	}
}
