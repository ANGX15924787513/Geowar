using Godot;
using System;

public partial class PointPlayer : RigidBody2D
{
	[Export] private int pointCount;
	[Export] private float circleRadius;
	[Export] private PackedScene singlePointScene;
	[Export] private float pullStrenth = 2f;

	private int _pointCount;
	GameManager gameManager;
	public override void _Ready()
	{
		SummonPoints();
		gameManager = GetNode<GameManager>("/root/GameManager");
	}
	
	public override void _Process(double delta)
	{
		GiveVelocityAll();
		Move(delta);
	}

	private Vector2 GetPositionInCircle(int index)
	{
		float angle = 2 * Single.Pi * index / pointCount;
		Vector2 offset = new Vector2(
			circleRadius * MathF.Sin(angle),
			circleRadius * MathF.Cos(angle)
		);
		return offset.Rotated(Rotation);
	}

	private void GiveVelocityAll()
	{
		int count = 0;
		var children = GetChildren();
		if(children == null) return;
		foreach (var node in children)
		{
			if (node is not RigidBody2D child) continue;
			Vector2 targetGlobal = ToGlobal(GetPositionInCircle(count));
			child.LinearVelocity = pullStrenth * (targetGlobal - child.GlobalPosition);
			count += 1;
		}
	}

	private void SummonPoints()
	{
		_pointCount = 0;
		while (_pointCount < pointCount){
			RigidBody2D point = (RigidBody2D)singlePointScene.Instantiate();
			point.Position = GetPositionInCircle(_pointCount);
			AddChild(point);
			_pointCount++;
		}
	}

	private void Move(double delta)
	{
		Vector2 velocity = gameManager.playerSpeed * Input.GetVector(
			"player_left",
			"player_right",
			"player_up",
			"player_down"
			);
		LinearVelocity = velocity;
		if (velocity.X != 0 && velocity.Y == 0)
		{
			AngularVelocity = velocity.X / gameManager.playerSpeed * gameManager.playerRotateSpeed;
		}else if (velocity.X == 0 && velocity.Y != 0)
		{
			AngularVelocity = velocity.Y / gameManager.playerSpeed * gameManager.playerRotateSpeed;
		}
	}
}
