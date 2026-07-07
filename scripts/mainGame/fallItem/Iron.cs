using Godot;
using System;

public partial class Iron : FallItem
{
    public override void _Ready()
    {
        base._Ready();
        itemType = FallItemType.IRON;
    }
}
