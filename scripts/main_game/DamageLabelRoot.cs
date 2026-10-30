using System.Collections.Generic;
using Godot;

public partial class DamageLabelRoot : Node2D
{
	[Export] private PackedScene damageLabelScene;

	private readonly Stack<Node2D> _free = new();
	public override void _Ready()
	{
		SignalManager.Instance.OnEnemyGotHurt += ShowDamageLabel;
	}

	public override void _ExitTree()
	{
		SignalManager.Instance.OnEnemyGotHurt -= ShowDamageLabel;
	}

	private void ShowDamageLabel(int damage, Vector2 pos)
	{
		Node2D label = _free.Count > 0 ? _free.Pop() : CreateLabel();
		label.GlobalPosition = pos;
		label.GetNode<Label>("DamageLabel").Text = damage.ToString();
		label.GetNode<AnimationPlayer>("AnimationPlayer").Play("show");
	}

	private Node2D CreateLabel()
	{
		var label = damageLabelScene.Instantiate<Node2D>();
		AddChild(label);
		label.GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += (_) => _free.Push(label);
		return label;
	}
}
