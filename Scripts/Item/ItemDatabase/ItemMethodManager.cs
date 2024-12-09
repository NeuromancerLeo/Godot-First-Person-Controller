using Godot;
using System;

public partial class ItemMethodManager : Node
{
    ItemNodeSceneSet _itemNodeSceneSet;
    CharacterBody3D _player;

    public override void _Ready()
    {
        //获取兄弟节点ItemNodeSceneSet的引用
        _itemNodeSceneSet = GetNode<ItemNodeSceneSet>("../ItemNodeSceneSet");

        //获取玩家Player节点的引用
        _player = GetNode<CharacterBody3D>("/root/World/Player");

    }

    public void ItemMethod(int id)
    {
        //C#字典当查找的键不存在时会返回字典的类型默认值，PackedScene的默认值为null
        //调用并储存返回来的值
        var value = _itemNodeSceneSet.GetNodeScene(id);
        //如果返回不为null
        if (value != null)
        {
            //则实例化返回的PackedSence
            //关于节点实例化的位置，目前我想到的，一般来说都是实例化到玩家节点身上吧？
            //武器的装备——玩家节点做父级；食物的动画——玩家节点做父级；
            //不管是长久驻留还是用完即删，都是在玩家Player节点下完成的，所以——
            //实例化返回的PackedSence到玩家Player节点下
            _player.AddChild(value.Instantiate());
            //然后就没了，剩下的方法由被实例化的节点↑进行操作。
        }
        //如果返回了null，
        //要么你忘记实现了:(
        //要么说明这个id的Item没有需要实例化节点场景来实现使用方法的任务
        //也就是说它没有Using/Equip方法
        else
        {
            //打印个警告就完事了
            GD.PrintErr("调用id为",id, "的Item的Using/Equip方法失败，因为ItemNodeSceneSet并没有查找到相关数据；如果该物品应该有这些方法，那么请检查你是否创建了对应的ItemNodeSceneResource并存放于正确的文件夹中确保ItemNodeSceneSet节点能顺利读取到它");

        }

    }







}

