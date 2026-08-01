using Godot;
using System;

public partial class Diamond : FallItem
{
    [Export] private AnimationPlayer lightAnim;
    public override void _Ready()
    {
        base._Ready();
        itemType = FallItemType.DIAMOND;
    }
}
