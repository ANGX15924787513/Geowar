using Godot;
using System;

public partial class Emerald : FallItem
{
    public override void _Ready()
    {
        base._Ready();
        itemType = FallItemType.EMERALD;
    }
}
