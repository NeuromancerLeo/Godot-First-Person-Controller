using Godot;
using System;

namespace ZombieWorldWalkDemo.Scripts.InGameMap.UI
{
    /// <summary>
    /// 管理进入正式游戏后的 UI 总节点，除了主菜单场景，每一个游戏地图场景都（需要）会有一个该节点来控制玩家进入地图后的 UI 界面.
    /// </summary>
    public partial class MasterUserInterface : Control
    {
        bool _isMenuOpened = false;
        bool _isDebugStatisticUIOpened = false;

        [Export]
        Control debugStatisticUI;

        void ChangeMouseMode()
        {
            //如果打开了菜单
            if (_isMenuOpened)
            {
                //鼠标释放
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                //锁定鼠标
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }

        public override void _Process(double delta)
        {
            //按下菜单键会使鼠标脱离锁定状态，再按一次会重新回到锁定状态
            if (Input.IsActionJustPressed("ui_menu"))
            {
                if (!_isMenuOpened)
                {
                    _isMenuOpened = true;
                    ChangeMouseMode();
                }
                else
                {
                    _isMenuOpened = false;
                    ChangeMouseMode();
                }
            }

            //按下 ui_debug 键会开启包含统计信息的UI
            if (Input.IsActionJustPressed("ui_debug"))
            {
                if (!_isDebugStatisticUIOpened)
                {
                    _isDebugStatisticUIOpened = true;
                    debugStatisticUI.Visible = true;
                }
                else
                {
                    _isDebugStatisticUIOpened = false;
                    debugStatisticUI.Visible = false;
                }

            }
        }

    }
}
