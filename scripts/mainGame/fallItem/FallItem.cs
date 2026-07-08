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
	GlobalAudioPlayer globalAudioPlayer;

	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
	}
	
	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("player"))
		{
			globalAudioPlayer.PlayAudio(globalAudioPlayer.collectXPSound);
			GD.Print($"掉落物{itemType}被捡起");
			signalManager.EmitSignal(SignalManager.SignalName.OnCollectedItem,(int)itemType);
			QueueFree();
		}
	}
}
