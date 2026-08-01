using Godot;
using System;

public partial class PointEnemy : Enemy
{
	protected override async void DiedHandler()
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return;
		GetNode<Area2D>("Area2D").CollisionMask &= ~(uint)(1 << (3 - 1));
		RemoveFromGroup("enemy");
		CollisionLayer &= ~(uint)(1 << (3 - 1));
		Tween hideTn = GetTree().CreateTween();
		hideTn.TweenProperty(this, "modulate", new Color(Colors.White, 0), 0.5f);
		await ToSignal(hideTn, "finished");
		signalManager.EmitSignal(SignalManager.SignalName.OnEnemyDied);
		signalManager.EmitSignal(SignalManager.SignalName.RequestCollectionSpawn, GlobalPosition);
		QueueFree();
	}
}
