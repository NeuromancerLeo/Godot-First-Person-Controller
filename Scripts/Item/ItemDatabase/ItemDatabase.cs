using Godot;
using System.Collections.Generic;
using System;

public partial class ItemDatabase : Node
{
    //这是存放所有ItemResource的文件夹路径
    [Export(PropertyHint.Dir)]
    string _itemResourceFolderDir;

    //包含了所有ItemResource的字典
    Dictionary<int,ItemResource> _itemDictionary = new Dictionary<int, ItemResource>();

    //子节点ItemMethodManager的引用
    ItemMethodManager _itemMethodManager;

    /// <summary>
    /// 加载并储存所有的ItemResource以初始化字典。
    /// </summary>
    void LoadItemResourceIntoDictionary()
    {
        //按照路径获取文件夹的引用(为DirAccess类)
        var _folder = DirAccess.Open(_itemResourceFolderDir);
        _folder.ListDirBegin();

        //DirAccess.GetNext()返回被访问目录下的文件名
        var _fileName = _folder.GetNext();

        while (_fileName != "")
        {
            if (_fileName != "readme.txt")
            {
                //GD.Print(fileName);
                //itemfolder为存放ItemResource的文件夹目录
                //fileName(itemResource文件的名字)
                ItemResource _item = GD.Load<ItemResource>(_itemResourceFolderDir + "/" + _fileName);
                //将在itemFolder文件夹下的所有ItemResource载入字典
                //键为ItemResource.id，值为ItemResource
                _itemDictionary.Add(_item.GetItemId(), _item);


                GD.Print("字典成功载入了：", _fileName, "！", "该Item的ID为：", _itemDictionary[_item.GetItemId()].GetItemId(), "，Type为：", _itemDictionary[_item.GetItemId()].GetItemType(), "，name为：", _itemDictionary[_item.GetItemId()].GetItemName(), "，最大堆叠数为：", _itemDictionary[_item.GetItemId()].GetItemMaximumStackQuantity(), "，Description为：", _itemDictionary[_item.GetItemId()]._description);

                _fileName = _folder.GetNext();
                //如此循环直至ItemResource全部根据其唯一id载入字典
            }
            else
            {
                _fileName = _folder.GetNext();
            }

        }

        _folder.ListDirEnd();
    }

    public override void _Ready()
    {
        LoadItemResourceIntoDictionary();

        _itemMethodManager = GetNode<ItemMethodManager>("ItemMethodManager");
    }

    public override void _Process(double delta)
    {
        /*
        //实验用
        if (Input.IsActionJustPressed("ui_left"))
        {
            UseItem(8);
        }
        */
        
    }

    public ItemResource GetItem(int id)
    {
        //根据id返回ItemResource
        return _itemDictionary[id];
    }

    //！！！待办事项（2024.10.06）！！！
    //UseItem方法不应该在此实现，而是应该转到Inventory内进行实现，具体实现即如下（已经验证可行）
    /*
    public void UseItem(int id)
    {
        //已经验证可行性！
        //把ItemId传入ItemMethodManager的ItemMethod()
        _itemMethodManager.ItemMethod(id);
    }
    */
}
