using Godot;
using System;

public partial class LinePlayer : Player
{
	[Export] private AudioStream chargeSound;
	[Export] private int lineCount;
	[Export] private int maxLineCount = 30;
	[Export] private PackedScene singleLineScene;
	[Export] public AudioStream attackSound;
	[Export] private float circleRadius = 120f;
	[Export] private int fireRate = 10;
	private int _currentLineCount;
	private int _launchedBullet;
	private Node2D bulletRoot;
	private float _fireTimer;
	
	private const float lineWidth = 20f;
	private const float gap = 8f;
	private const float spacing = lineWidth + gap;
	private bool isPreLaunching;

	private int throughUgLevel;
	public override void _Ready()
	{
		base._Ready();
		bulletRoot = GetTree().CurrentScene.GetNode<Node2D>("bulletRoot");
		SummonLines();
		ApplyShopUpgrades();
		lineCount = Math.Min(lineCount, maxLineCount);
		throughUgLevel = dataManager.GetUpgradeLevel("through");
		
		signalManager.OnPlayerDied += OnLineDied;
		signalManager.OnPlayerHealthChanged += OnHealthChanged;
		signalManager.OnBulletDestroyed += SpawnLine;
		signalManager.OnBulletDestroyed += DownLaunchedBullet;
	}
	
	public override void _Process(double delta)
	{
		//右键 -> ToPreLaunchPos -> GetPositionInBulletSide/Lib
		//else -> GiveVelocityAll OK
		base._Process(delta);
		if (gameManager.gameState != GameManager.GameState.GAMING)
		{
			LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, 0.05f);
			return;
		}
		if (Input.IsActionPressed("pre_launch_bullet"))
		{
			ToPreLaunchPos();
			if(!isPreLaunching)
			{
				isPreLaunching = true;
				globalAudioPlayer.PlayAudio(chargeSound);
			}
		}
		else
		{
			GiveVelocityAll((float)delta);
			isPreLaunching = false;
		}

		AngularVelocity = -Rotation;

		if (CanMove())
		{
			ApplyMovement(gameManager.playerSpeed);
		}
		else
			Decelerate(delta);
		if (CanLaunchBullet())
		{
			if (Input.IsActionPressed("shift") && _currentLineCount == lineCount)
			{
				LaunchAllBullet();
			}else if(!Input.IsActionPressed("shift"))
			{
				LaunchBullet();
			}
		}

		if (_launchedBullet == 0 && _currentLineCount != lineCount)
		{
			for (int i = 0; i < lineCount - _currentLineCount; i++)
			{
				SpawnLine();
			}
		}

		if (lineCount > maxLineCount)
		{
			lineCount = maxLineCount;
		}
		if (_currentLineCount > lineCount)
		{
			_currentLineCount = lineCount;
		}
	}

	private void SummonLines()
	{
		_currentLineCount = lineCount;
		for (int i = 0; i < _currentLineCount - 2; i++)
		{
			var p = (Node2D)singleLineScene.Instantiate();
			p.Position = GetPositionInCircle(i);
			AddChild(p);
		}

		for (int i = 0; i < 2; i++)
		{
			var p = (Node2D)singleLineScene.Instantiate();
			p.Position = GetPositionInBulletSide(i);
			p.AddToGroup("lineSide");
			AddChild(p);
		}
	}

	private void ToPreLaunchPos()
	{
		int count = 0;
		int ctSide = 0;
		foreach (var node in GetChildren())
		{
			if (node is not Line child) continue;
			if (child.IsInGroup("lineSide"))
			{
				Vector2 target = GetPositionInBulletSide(ctSide);
				// child.LinearVelocity = 2 * (target - child.GlobalPosition);
				child.Position = child.Position.Lerp(target, 0.05f);
				ctSide++;
			}
			else
			{
				Vector2 target = GetPositionInBulletLib(count);
				// child.LinearVelocity = 2 * (target - child.GlobalPosition);
				child.Position = child.Position.Lerp(target, 0.05f);
				count++;
			}
			// 计算目标角度
			float targetAngle = Mathf.Pi / 2 + (GetGlobalMousePosition() - GlobalPosition).Angle();

// 最短角度差
			float diff = targetAngle - child.Rotation;
			diff = (diff + Mathf.Pi * 3) % (Mathf.Pi * 2) - Mathf.Pi; // 限制到 [-π, π]

// Lerp 差值后的新角度
			child.Rotation += diff * 0.1f;
		}
	}

	private Vector2 GetPositionInBulletLib(int index)
	{
		Vector2 mouseDir = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		int libSize = _currentLineCount - 2;

		float halfWidth = (libSize - 1) * spacing / 2f;   // 仍用于计算侧边线
		float fixedVerticalOffset = 0f;                 // 常量微调，不随 lineCount 变化

		Vector2 offset = -mouseDir * 100f
		                 + mouseDir.Rotated(Mathf.Pi / 2) * (index * spacing - halfWidth + fixedVerticalOffset);
		return offset;
	}

	private Vector2 GetPositionInBulletSide(int index)
	{
		Vector2 mouseDir = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		Vector2 leftDir = new Vector2(-mouseDir.Y, mouseDir.X);
		int libSize = _currentLineCount - 2;
    
		float halfWidth = (libSize - 1) * spacing / 2f;
		float sideFromCenter = halfWidth + spacing;  // 最边缘 bullet + 一个间距
		if (sideFromCenter < 40f)
			sideFromCenter = 40f;  // 最小半间距，保证 side 间距 ≥ 80px
    
		return index switch
		{
			0 => -leftDir * sideFromCenter,
			1 =>  leftDir * sideFromCenter,
			_ => Vector2.Zero
		};
	}
	
	private Vector2 GetPositionInCircle(int index)
	{
		float angle = 2 * MathF.PI * index / _currentLineCount;
		return new Vector2(
			circleRadius * MathF.Sin(angle),
			circleRadius * MathF.Cos(angle)
		);
	}

	private void SpawnLine()
	{
			_currentLineCount++;
			var p = (Node2D)singleLineScene.Instantiate();
			p.Position = GetPositionInCircle(_currentLineCount);
			CallDeferred(Node.MethodName.AddChild, p);
	}

	private void GiveVelocityAll(float delta)
	{
		int count = 0;
		foreach (var node in GetChildren())
		{
			if (node is not Line child) continue;
			Vector2 target = GetPositionInCircle(count);
			child.Position = child.Position.Lerp(target, 0.05f);
			child.Rotate(3*Single.Pi/2*delta);
			count++;
		}
	}

	private void OnLineDied()
	{
		circleRadius = 0f;
	}

	private void DownLaunchedBullet()
	{
		_launchedBullet--;
	}

	private bool CanLaunchBullet()
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return false;
		if (_currentLineCount <= 2) return false;
		if (!(Input.IsActionPressed("player_launch_bullet") && Input.IsActionPressed("pre_launch_bullet"))) return false;

		_fireTimer += (float)GetProcessDeltaTime();
		float interval = 1f / fireRate;
		if (_fireTimer < interval) return false;

		_fireTimer -= interval;
		return true;
	}

	private void LaunchBullet()
	{
		_launchedBullet++;
		_fireRotationTimer = 0.5f;
		Line bullet = null;
		foreach (var b in GetChildren())
		{
			if (b is Line && !b.IsInGroup("lineSide"))
			{
				bullet = b as Line;
				break;
			}
		}
		if (bullet == null) return;

		Vector2 gPos = bullet.GlobalPosition;
		var aimPos = Cursor.Instance?.AimPosition ?? GetGlobalMousePosition();
		Vector2 vel = (aimPos - gPos).Normalized() * 1000f;
		bullet.GetParent().RemoveChild(bullet);
		_currentLineCount--;
		bulletRoot.AddChild(bullet);
		bullet.GlobalPosition = gPos;
		bullet.flySpeed = vel;  // AddChild 之后设，不怕 reparent 清掉
		bullet.穿透等级 += 2*throughUgLevel;
		
		globalAudioPlayer.PlayAudio(attackSound);
	}

	private void LaunchAllBullet()
	{
		int willLaunch = _currentLineCount - 2;
		_launchedBullet += willLaunch;
		_fireRotationTimer = 0.5f;
		_currentLineCount -= willLaunch;
		for (int i = 0; i < willLaunch; i++)
		{
			Line bullet = null;
			foreach (var b in GetChildren())
			{
				if (b is Line && !b.IsInGroup("lineSide"))
				{
					bullet = b as Line;
					break;
				}
			}
			if (bullet == null) return;

			Vector2 gPos = bullet.GlobalPosition;
			var aimPos = Cursor.Instance?.AimPosition ?? GetGlobalMousePosition();
			Vector2 vel = (aimPos - gPos).Normalized() * 1000f;
			bullet.GetParent().RemoveChild(bullet);
			bulletRoot.AddChild(bullet);
			bullet.GlobalPosition = gPos;
			bullet.flySpeed = vel;  // AddChild 之后设，不怕 reparent 清掉
			bullet.穿透等级 += 2*throughUgLevel;
		}
		globalAudioPlayer.PlayAudio(attackSound);
	}

	private void ApplyShopUpgrades()
	{
		int ct = dataManager.GetUpgradeLevel("line_count");//
		int fr = dataManager.GetUpgradeLevel("fire_rate");//
		int ms = dataManager.GetUpgradeLevel("move_speed");//
		int atk = dataManager.GetUpgradeLevel("atk_hp");//
		int hp = dataManager.GetUpgradeLevel("max_hp");//
		int ls = dataManager.GetUpgradeLevel("lifesteal");//

		lineCount = 5 + ct;
		Line.AttackBonus = atk;
		fireRate += fr;
		gameManager.playerSpeed += ms * 20;
		HP += hp * 5;
		_hp = HP;

		if (ls > 0)
			signalManager.OnEnemyDied += OnLifeSteal;

		// 升级后重新通知血条
		signalManager.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged, 0, _hp, HP);
	}

	public void ApplyMovement(float speed)
	{
		Vector2 velocity = speed * Input.GetVector(
			"player_left", "player_right", "player_up", "player_down");
		if (velocity != Vector2.Zero)
		{
			LinearVelocity = LinearVelocity.Lerp(velocity,rotateSmooth);
		}
		else
		{
			LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, slowDownSmooth);
		}
	}
	
	public override void _ExitTree()
	{
		signalManager.OnPlayerDied -= OnLineDied;
		signalManager.OnPlayerHealthChanged -= OnHealthChanged;
		signalManager.OnBulletDestroyed -= SpawnLine;
		signalManager.OnBulletDestroyed -= DownLaunchedBullet;
		base._ExitTree();
	}
}
