using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// 载有Item Using或Equip方法的节点场景所有集
/// </summary>
public partial class ItemNodeSceneSet : Node
{
    //这是存放所有ItemNodeSceneResource的文件夹路径
    [Export(PropertyHint.Dir)]
    string _itemNodeSceneResourceFolderDir;

    //字典，键为Item的id，值为对应的ItemNodeScene
    Dictionary<int,PackedScene> _itemNodeSceneDictionary = new Dictionary<int,PackedScene>();

    /// <summary>
    /// 加载并储存所有的ItemNodeSceneResource以初始化字典。
    /// </summary>
    void LoadSceneResourceIntoDictionary()
    {
        var _folder = DirAccess.Open(_itemNodeSceneResourceFolderDir);
        _folder.ListDirBegin();

        var _fileName = _folder.GetNext();


        while (_fileName != "")
        {
            if (_fileName != "readme.txt")
            {
                //GD.Print(fileName);
                //加载Resource文件
                dynamic _senceResource = GD.Load<ItemNodeSceneResource>(_itemNodeSceneResourceFolderDir + "/" + _fileName);
                //载入字典，键为id，值为PackedScene
                _itemNodeSceneDictionary.Add(_senceResource.GetSceneId(), _senceResource.GetScene());

                GD.Print("ItemNodeScene字典成功载入了：", _fileName, "！", "该Scene为：", _itemNodeSceneDictionary[_senceResource.GetSceneId()]);

                _fileName = _folder.GetNext();
                //如此循环直至ItemResource全部根据其唯一id载入字典
            }
            else
            {
                _fileName = _folder.GetNext();
            }
        }
    }

    public override void _Ready()
    {
        LoadSceneResourceIntoDictionary();
    }

    /// <summary>
    /// 内部使用字典以传入的id为键进行查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public PackedScene GetNodeScene(int id)
    {
        return _itemNodeSceneDictionary[id];
    }
}
