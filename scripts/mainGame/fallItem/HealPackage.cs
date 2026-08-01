using Godot;
using System;

public partial class HealPackage : Node2D
{
	[Export] private int healHp = 5;
	[Export] private AnimationPlayer lightAnim;
	public override void _Ready()
	{
		
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.GotHeal(healHp);
			Area2D area2D = GetNode<Area2D>("Sprite2D/Area2D");
			area2D.CollisionLayer = 0;
			area2D.CollisionMask = 0;
			GetNode<AnimationPlayer>("AnimationPlayer").Play("fallItemAni/collected");
		}
	}

	private void Highlight()
	{
		lightAnim.Play("highlight");
	}
}
