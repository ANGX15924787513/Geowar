using Godot;
using System;
using Godot.Collections;

public partial class PointPlayer : Player
{
	[Export] private int pointCount;
	[Export] private int maxPointCount = 15;
	[Export] private float circleRadius;
	[Export] private PackedScene singlePointScene;
	[Export] private float pullStrenth = 2f;
	[Export] private float fireRotateSpeed = 3f;
	[Export] private int fireRate = 10;
	private float _fireTimer;
	[Export] private PackedScene hpBarScene;
	[Export] public AudioStream attackSound;
	[Export] public AudioStream hurtSound;

	private int _currentPointCount;
	private int _launchedBullet;
	private Node2D bulletRoot;

	public override void _Ready()
	{
		SummonHpBar();
		bulletRoot = GetTree().CurrentScene.GetNode<Node2D>("bulletRoot");
		base._Ready();
		SummonPoints();
		signalManager.OnPlayerDied += OnPointDie;
		signalManager.OnPlayerHealthChanged += OnGotHurt;
		signalManager.OnBulletDestroyed += SpawnPoint;
		signalManager.OnBulletDestroyed += DownLaunchedBullet;
	}

	private void OnGotHurt(int damage, int health, int maxHealth)
	{
		if (damage <= 0) return;
		globalAudioPlayer.PlayAudio(hurtSound);
	}

	private void SummonHpBar()
	{
		var scene = hpBarScene.Instantiate();
		GetTree().CurrentScene.AddChild(scene);
	}

	private void OnPointDie()
	{
		circleRadius = 0f;
	}

	private void DownLaunchedBullet()
	{
		_launchedBullet--;
	}

	public override void _ExitTree()
	{
		signalManager.OnPlayerDied -= OnPointDie;
		signalManager.OnPlayerHealthChanged -= OnGotHurt;
		signalManager.OnBulletDestroyed -= SpawnPoint;
		signalManager.OnBulletDestroyed -= DownLaunchedBullet;
		base._ExitTree();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		GiveVelocityAll();

		if (CanMove())
			ApplyMovement(gameManager.playerSpeed, gameManager.playerRotateSpeed);
		else
			Decelerate(delta);

		if (CanLaunchBullet())
			LaunchBullet();

		if (_launchedBullet == 0 && _currentPointCount != pointCount)
		{
			for (int i = 0; i < pointCount - _currentPointCount; i++)
			{
				SpawnPoint();
			}
		}

		if (pointCount > maxPointCount)
		{
			pointCount = maxPointCount;
		}
		if (_currentPointCount > pointCount)
		{
			_currentPointCount = pointCount;
		}
	}

	private bool CanLaunchBullet()
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return false;
		if (_currentPointCount <= 1) return false;
		if (!Input.IsActionPressed("player_launch_bullet")) return false;

		_fireTimer += (float)GetProcessDeltaTime();
		float interval = 1f / fireRate;
		if (_fireTimer < interval) return false;

		_fireTimer -= interval;
		return true;
	}

	private void LaunchBullet()
	{
		_launchedBullet++;
		AngularVelocity = fireRotateSpeed;
		_fireRotationTimer = 0.5f;

		var bullet = GetClosestPoint(GetPoints(), GetTangentPoint());
		Vector2 globalPos = bullet.GlobalPosition;
		bullet.GetParent().RemoveChild(bullet);
		_currentPointCount--;
		bulletRoot.AddChild(bullet);
		bullet.GlobalPosition = globalPos;
		bullet.LinearVelocity = GetGlobalMousePosition() - bullet.GlobalPosition;
		
		globalAudioPlayer.PlayAudio(attackSound);
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
		p.Position = GetPositionInCircle(_currentPointCount);
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
