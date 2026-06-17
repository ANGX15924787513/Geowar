using Godot;
using System;

public partial class PointBullet : CharacterBody2D
{
	[Export] private float lifetime = 5f;
	[Export] private float damage = 10f;
	[Export] private float speed = 10f;
	float _lifetime;
	public override void _Ready()
	{
		_lifetime = 0f;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_lifetime > lifetime) QueueFree();
		_lifetime += (float)delta;
		Velocity = Vector2.Up.Rotated(Rotation) *  speed;
		MoveAndSlide();
	}

	private void _on_area_2d_body_entered(Node2D body)
	{
		GD.Print($"子弹打到{body.Name}");
		if (body is StaticBody2D) QueueFree();
		if (body.IsInGroup("enemy"))
		{
			//TODO
		}
	}
}
