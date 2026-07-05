using Godot;
using System;

public partial class PointEnemy : Enemy
{
	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			((Player)body).GotHurt(Damage);
		}
	}
}
