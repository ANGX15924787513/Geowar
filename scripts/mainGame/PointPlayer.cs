using Godot;
using System;
using Godot.Collections;

public partial class PointPlayer : Player
{
	[Export] private int pointCount;
	[Export] private float circleRadius;
	[Export] private PackedScene singlePointScene;
	[Export] private float pullStrenth = 2f;
	[Export] private float fireRotateSpeed = 3f;

	private int _currentPointCount;
	private AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		base._Ready();
		SummonPoints();
		signalManager.OnPlayerDied += OnPointDie;
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		signalManager.OnBulletDestroyed += SpawnPoint;
	}

	private void OnPointDie()
	{
		if (!IsInstanceValid(animationPlayer)) return;
		animationPlayer.GetAnimation("on_die").TrackSetKeyValue(0, 1, circleRadius);
		animationPlayer.Play("on_die");
		GD.Print("point die anim played");
	}

	public override void _ExitTree()
	{
		signalManager.OnPlayerDied -= OnPointDie;
		signalManager.OnBulletDestroyed -= SpawnPoint;
		base._ExitTree();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		GiveVelocityAll();

		if (CanMove())
			ApplyMovement(gameManager.playerSpeed, gameManager.playerRotateSpeed);

		if (CanLaunchBullet())
			LaunchBullet();
	}

	// ==================== 发射 ====================

	private bool CanLaunchBullet()
	{
		return
			gameManager.gameState == GameManager.GameState.GAMING &&
			Input.IsActionJustPressed("player_launch_bullet") &&
			_currentPointCount > 1;
	}

	private void LaunchBullet()
	{
		AngularVelocity = fireRotateSpeed;
		_fireRotationTimer = 0.5f;

		var bullet = GetClosestPoint(GetPoints(), GetTangentPoint());
		Vector2 globalPos = bullet.GlobalPosition;
		bullet.GetParent().RemoveChild(bullet);
		_currentPointCount--;
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = globalPos;
		bullet.LinearVelocity = GetGlobalMousePosition() - bullet.GlobalPosition;
	}

	private RigidBody2D GetClosestPoint(Array<RigidBody2D> points, Vector2 target)
	{
		RigidBody2D closest = null;
		float min = float.MaxValue;
		foreach (var p in points)
		{
			float d = p.GlobalPosition.DistanceSquaredTo(target);
			if (d < min) { min = d; closest = p; }
		}
		return closest;
	}

	private Array<RigidBody2D> GetPoints()
	{
		var rst = new Array<RigidBody2D>();
		foreach (var node in GetChildren())
			if (node is RigidBody2D rb)
				rst.Add(rb);
		return rst;
	}

	/// <summary> 鼠标 → 圆心 的顺时针切点 </summary>
	private Vector2 GetTangentPoint()
	{
		Vector2 mouse = GetGlobalMousePosition();
		Vector2 dir = (mouse - GlobalPosition).Normalized();
		float dist = mouse.DistanceTo(GlobalPosition);

		if (dist <= circleRadius)
			return GlobalPosition + dir * circleRadius;

		float alpha = Mathf.Acos(circleRadius / dist);
		return GlobalPosition + dir.Rotated(-alpha) * circleRadius;
	}

	// ==================== 点圈管理 ====================

	private void SummonPoints()
	{
		_currentPointCount = pointCount;
		for (int i = 0; i < _currentPointCount; i++)
		{
			var p = (RigidBody2D)singlePointScene.Instantiate();
			p.Position = GetPositionInCircle(i);
			AddChild(p);
		}
	}

	private void SpawnPoint()
	{
		_currentPointCount++;
		var p = (RigidBody2D)singlePointScene.Instantiate();
		p.Position = GetPositionInCircle(_currentPointCount - 1);
		CallDeferred(Node.MethodName.AddChild, p);
	}

	private Vector2 GetPositionInCircle(int index)
	{
		float angle = 2 * MathF.PI * index / _currentPointCount;
		return new Vector2(
			circleRadius * MathF.Sin(angle),
			circleRadius * MathF.Cos(angle)
		).Rotated(Rotation);
	}

	private void GiveVelocityAll()
	{
		int count = 0;
		foreach (var node in GetChildren())
		{
			if (node is not RigidBody2D child) continue;
			Vector2 target = ToGlobal(GetPositionInCircle(count));
			child.LinearVelocity = pullStrenth * (target - child.GlobalPosition);
			count++;
		}
	}
}
