using Godot;
using System;

public partial class FallItem : Node2D
{
	public enum FallItemType{
		IRON,
		GOLD,
		DIAMOND,
		EMERALD
	}
	public FallItemType itemType;

	SignalManager signalManager;

	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
	}
	
	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("player"))
		{
			GD.Print("掉落物被捡起");
			signalManager.EmitSignal(SignalManager.SignalName.OnCollectedItem,(int)itemType);
			QueueFree();
		}
	}
}
