using Godot;
using System;

[GlobalClass]
public partial class ItemNodeSceneResource : Resource
{
    [Export]
    int _id;
    [Export]
    PackedScene _nodeScene;

    public int GetSceneId()
    {
        return _id;
    }

    public PackedScene GetScene()
    {
        return _nodeScene;
    }

}
