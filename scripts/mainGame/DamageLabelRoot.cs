using Godot;
using System;

public partial class DamageLabelRoot : Node2D
{
	[Export] private PackedScene damageLabelScene;
	private SignalManager signalManager;
	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		signalManager.OnEnemyGotHurt += ShowDamageLabel;
	}

	public override void _ExitTree()
	{
		signalManager.OnEnemyGotHurt -= ShowDamageLabel;
	}

	private void ShowDamageLabel(int damage, Vector2 pos)
	{
		var scene = damageLabelScene.Instantiate() as Node2D;
		scene.GlobalPosition = pos;
		scene.GetNode<Label>("DamageLabel").Text = damage.ToString();
		AddChild(scene);
	}
}
