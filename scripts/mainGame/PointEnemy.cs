using Godot;
using System;

public partial class PointEnemy : Enemy
{
	private AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		base._Ready();
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	protected override async void DiedHandler()
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return;
		GetNode<Area2D>("Area2D").CollisionMask &= ~(uint)(1 << (3 - 1));
		RemoveFromGroup("enemy");
		CollisionLayer &= ~(uint)(1 << (3 - 1));
		animationPlayer.Play("pointDie");
		await ToSignal(animationPlayer, "animation_finished");
		signalManager.EmitSignal(SignalManager.SignalName.OnEnemyDied);
			signalManager.EmitSignal(SignalManager.SignalName.RequestCollectionSpawn, GlobalPosition);
		QueueFree();
	}
}
