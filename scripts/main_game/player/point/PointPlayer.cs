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
	[Export] public AudioStream attackSound;

	private int _currentPointCount;
	private int _launchedBullet;
	private Node2D bulletRoot;

	public override void _Ready()
	{
		bulletRoot = GetTree().CurrentScene.GetNode<Node2D>("bulletRoot");
		base._Ready();
		currentPlayerType = PlayerTypeEnum.POINT;
		ApplyShopUpgrades();
		SummonPoints();
		
		SignalManager.Instance.OnPlayerDied += OnPointDie;
		SignalManager.Instance.OnPlayerHealthChanged += OnHealthChanged;
		SignalManager.Instance.OnBulletDestroyed += SpawnPoint;
		SignalManager.Instance.OnBulletDestroyed += DownLaunchedBullet;
	}

	private void OnPointDie()
	{
		circleRadius = 0f;
	}

	private void DownLaunchedBullet()
	{
		_launchedBullet--;
	}

	// ==================== 商店升级应用 ====================

	private void ApplyShopUpgrades()
	{
		int pc = DataManager.Instance.GetUpgradeLevel("point_count");
		int fr = DataManager.Instance.GetUpgradeLevel("fire_rate");
		int ms = DataManager.Instance.GetUpgradeLevel("move_speed");
		int atk = DataManager.Instance.GetUpgradeLevel("atk_hp");
		int hp = DataManager.Instance.GetUpgradeLevel("max_hp");
		int ls = DataManager.Instance.GetUpgradeLevel("lifesteal");

		PointSync.AttackBonus = atk;
		pointCount           += pc;
		fireRate             += fr;
		GameManager.Instance.playerSpeed += ms * 20;
		HP                   += hp * 10;
		_hp                   = HP;

		if (ls > 0)
			SignalManager.Instance.OnEnemyDied += OnLifeSteal;

		// 升级后重新通知血条
		SignalManager.Instance.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged, 0, _hp, HP);
	}

	public override void _ExitTree()
	{
		SignalManager.Instance.OnPlayerDied -= OnPointDie;
		SignalManager.Instance.OnPlayerHealthChanged -= OnHealthChanged;
		SignalManager.Instance.OnBulletDestroyed -= SpawnPoint;
		SignalManager.Instance.OnBulletDestroyed -= DownLaunchedBullet;
		base._ExitTree();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (CanLaunchBullet())
		{
			if (Input.IsActionPressed("shift") && _currentPointCount == pointCount)
			{
				LaunchBigBullet();
			}
			else if (!Input.IsActionPressed("shift"))
			{
				LaunchBullet();
			}
		}

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

	public override void _PhysicsProcess(double delta)
	{
		GiveVelocityAll();

		if (CanMove() && !isInvincible)
			ApplyMovement(GameManager.Instance.playerSpeed, GameManager.Instance.playerRotateSpeed);
		else
			Decelerate(delta);
	}

	private bool CanLaunchBullet()
	{
		if (GameManager.Instance.gameState != GameManager.GameState.GAMING) return false;
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
		var aimPos = Cursor.Instance?.AimPosition ?? GetGlobalMousePosition();
		bullet.LinearVelocity = aimPos - bullet.GlobalPosition;
		
		GlobalAudioPlayer.Instance.PlayAudio(attackSound);
	}

	private void LaunchBigBullet()
	{
		int willLaunch = _currentPointCount - 1;
		_launchedBullet += willLaunch;
		AngularVelocity = fireRotateSpeed;
		_fireRotationTimer = 0.5f;
		_currentPointCount -= willLaunch;
		for (int i = 0; i < willLaunch; i++)
		{
			var bullet = GetClosestPoint(GetPoints(), GetTangentPoint());
			Vector2 globalPos = bullet.GlobalPosition;
			bullet.GetParent().RemoveChild(bullet);
			bulletRoot.AddChild(bullet);
			bullet.GlobalPosition = globalPos;
			var aimPos = Cursor.Instance?.AimPosition ?? GetGlobalMousePosition();
			bullet.LinearVelocity = aimPos - bullet.GlobalPosition;
		}
		GlobalAudioPlayer.Instance.PlayAudio(attackSound);
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
		Vector2 mouse = Cursor.Instance?.AimPosition ?? GetGlobalMousePosition();
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
		int childCount = GetChildCount();
		for (int i = 0; i < childCount; i++)
		{
			var node = GetChild(i);
			if (node is not RigidBody2D child) continue;
			Vector2 target = ToGlobal(GetPositionInCircle(count));
			child.LinearVelocity = pullStrenth * (target - child.GlobalPosition);
			count++;
		}
	}
}
