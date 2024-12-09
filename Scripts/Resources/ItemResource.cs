using Godot;
using System;

[GlobalClass]
public partial class ItemResource : Resource
{
    [Export]
    private int _id = 0;
    [Export(PropertyHint.Enum, "Firearms,Modification parts,Cold Arms,Ammo,Food,Medical supplies,Equipment,Tool")]
    private string _type;
    [Export]
    private string _name;
    [Export]
    public PackedScene modelScene;
    [Export]
    private int _maximumStackQuantity;
    
    [Export(PropertyHint.MultilineText)]
    public string _description;


    public int GetItemId()
    {
        return _id; 
    }

    public string GetItemType()
    {
        return _type;
    }

    public string GetItemName()
    {
        return _name;
    }

    public int GetItemMaximumStackQuantity()
    {
        return _maximumStackQuantity;
    }

    public string GetItemDescription()
    {
        return _description;
    }

    

}

