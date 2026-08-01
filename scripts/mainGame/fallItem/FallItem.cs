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
	
	[Export]private AnimationPlayer Anim;

	SignalManager signalManager;
	GlobalAudioPlayer globalAudioPlayer;

	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
		if (Anim == null)
		{
			Anim = GetNode<AnimationPlayer>("AnimationPlayer");
		}
	}
	
	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("player") || (body.IsInGroup("circle_point") && ((PointSync)body).pointType == PointSync.PointType.CIRCLE))
		{
			globalAudioPlayer.PlayAudio(globalAudioPlayer.collectXPSound);
			// GD.Print($"掉落物 {itemType} 被捡起");
			signalManager.EmitSignal(SignalManager.SignalName.OnCollectedItem,(int)itemType);
			Area2D area2D = GetNode<Area2D>("Sprite2D/Area2D");
			area2D.CollisionLayer = 0;
			area2D.CollisionMask = 0;
			Anim.Play("fallItemAni/collected");
		}
	}
}
