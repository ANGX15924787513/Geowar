using Godot;
using System;

public partial class Gold : FallItem
{
    public override void _Ready()
    {
        base._Ready();
        itemType = FallItemType.GOLD;
    }
}
