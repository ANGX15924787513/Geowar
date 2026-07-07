using Godot;
using System;

public partial class Diamond : FallItem
{
    public override void _Ready()
    {
        base._Ready();
        itemType = FallItemType.DIAMOND;
    }
}
