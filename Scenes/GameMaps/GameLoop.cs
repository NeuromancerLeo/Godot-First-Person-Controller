using Godot;
using System;

public partial class GameLoop : Node
{
    bool isMenuOpened = false;
    public override void _Process(double delta)
    {
        //检查是否按下菜单键
        if (Input.IsActionJustPressed("ui_menu"))
        {
            if (!isMenuOpened)
            {
                isMenuOpened = true;
                ChangeMouseMode();
            }
            else
            {
                isMenuOpened = false;
                ChangeMouseMode();
            }     
        }

        
    }

    void ChangeMouseMode()
    {
        //如果打开了菜单
        if (isMenuOpened)
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
}
