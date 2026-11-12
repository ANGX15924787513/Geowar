using Godot;
using System;
using System.Collections.Generic;

public partial class LinePlayer : Player
{
	[Export] private AudioStream chargeSound;
	[Export] private int lineCount;
	[Export] public int maxLineCount = 30;
	[Export] private PackedScene singleLineScene;
	[Export] public AudioStream attackSound;
	[Export] private float circleRadius = 120f;
	[Export] private int fireRate = 10;
	private int _currentLineCount;
	private int _launchedBullet;
	private Node2D bulletRoot;
	private float _fireTimer;

	private int _baseLineCount;
	private int _superMagazineBonus;
	
	private const float lineWidth = 20f;
	private const float gap = 8f;
	private const float spacing = lineWidth + gap;
	private bool isPreLaunching;

	private int throughUgLevel;

	public int fireDamage;
	public override void _Ready()
	{
		base._Ready();
		currentPlayerType = PlayerTypeEnum.LINE;
		bulletRoot = GetTree().CurrentScene.GetNode<Node2D>("bulletRoot");
		SummonLines();
		ApplyShopUpgrades();
		lineCount = Math.Min(lineCount, maxLineCount);
		_baseLineCount = lineCount;
		throughUgLevel = DataManager.Instance.GetUpgradeLevel("through");
		fireDamage = 2 * DataManager.Instance.GetUpgradeLevel("line_fire");
		
		SignalManager.Instance.OnPlayerDied += OnLineDied;
		SignalManager.Instance.OnPlayerHealthChanged += OnHealthChanged;
		SignalManager.Instance.OnBulletDestroyed += SpawnLine;
		SignalManager.Instance.OnBulletDestroyed += DownLaunchedBullet;
	}
	
	public override void _Process(double delta)
	{
		//右键 -> ToPreLaunchPos -> GetPositionInBulletSide/Lib
		//else -> GiveVelocityAll OK
		if (GameManager.Instance.gameState != GameManager.GameState.GAMING)
		{
			return;
		}
		base._Process(delta);
		if (CanLaunchBullet())
		{
				if (Input.IsActionPressed("shift") && _currentLineCount == lineCount)
				{
					LaunchAllBullet();
				}
				else if (!Input.IsActionPressed("shift"))
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

		int lineCap = maxLineCount + _superMagazineBonus;
		if (lineCount > lineCap)
		{
			lineCount = lineCap;
		}
		if (_currentLineCount > lineCount)
		{
			_currentLineCount = lineCount;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GameManager.Instance.gameState != GameManager.GameState.GAMING)
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
				GlobalAudioPlayer.Instance.PlayAudio(chargeSound);
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
			ApplyMovement(GameManager.Instance.playerSpeed);
		}
		else
			Decelerate(delta);
	}

	private void SummonLines()
	{
		_currentLineCount = lineCount;
		for (int i = 0; i < _currentLineCount - 2; i++)
		{
			var p = singleLineScene.Instantiate<Line>();
			p.parent = this;
			p.Position = GetPositionInCircle(i);
			AddChild(p);
		}

		Vector2 summonMouseDir = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		for (int i = 0; i < 2; i++)
		{
			var p = (Node2D)singleLineScene.Instantiate();
			p.Position = GetPositionInBulletSide(i, summonMouseDir);
			p.AddToGroup("lineSide");
			AddChild(p);
		}
	}

	private void ToPreLaunchPos()
	{
		int count = 0;
		int ctSide = 0;
		Vector2 mouseDir = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		int childCount = GetChildCount();
		for (int i = 0; i < childCount; i++)
		{
			var node = GetChild(i);
			if (node is not Line child) continue;
			if (child.IsInGroup("lineSide"))
			{
				Vector2 target = GetPositionInBulletSide(ctSide, mouseDir);
				// child.LinearVelocity = 2 * (target - child.GlobalPosition);
				child.Position = child.Position.Lerp(target, 0.08f);
				ctSide++;
			}
			else
			{
				Vector2 target = GetPositionInBulletLib(count, mouseDir);
				// child.LinearVelocity = 2 * (target - child.GlobalPosition);
				child.Position = child.Position.Lerp(target, 0.08f);
				count++;
			}
			// 计算目标角度
			float targetAngle = Mathf.Pi / 2 + mouseDir.Angle();

// 最短角度差
			float diff = targetAngle - child.Rotation;
			diff = (diff + Mathf.Pi * 3) % (Mathf.Pi * 2) - Mathf.Pi; // 限制到 [-π, π]

// Lerp 差值后的新角度
			child.Rotation += diff * 0.1f;
		}
	}

	private Vector2 GetPositionInBulletLib(int index, Vector2 mouseDir)
	{
		int libSize = _currentLineCount - 2;

		float halfWidth = (libSize - 1) * spacing / 2f;   // 仍用于计算侧边线
		float fixedVerticalOffset = 0f;                 // 常量微调，不随 lineCount 变化

		Vector2 offset = -mouseDir * 100f
		                 + mouseDir.Rotated(Mathf.Pi / 2) * (index * spacing - halfWidth + fixedVerticalOffset);
		return offset;
	}

	private Vector2 GetPositionInBulletSide(int index, Vector2 mouseDir)
	{
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

	// ==================== 超级弹夹 ====================

	protected override void OnSuperMagazineStart()
	{
		int mult = Mathf.Max(1, superMagazineMultiplier);
		_superMagazineBonus = _baseLineCount * (mult - 1);
		lineCount = _baseLineCount + _superMagazineBonus;
		FillLinesToCapacity();
	}

	protected override void OnSuperMagazineEnd()
	{
		_superMagazineBonus = 0;
		lineCount = _baseLineCount;
		TrimSurplusLines();
	}

	/// <summary> 超级弹夹结束后清掉多出来的子弹，保证数量和容量一致（侧边两条线保留） </summary>
	private void TrimSurplusLines()
	{
		int targetLibCount = Mathf.Max(0, lineCount - 2);
		var libLines = new List<Line>();
		foreach (Node child in GetChildren())
		{
			if (child is Line line && !line.IsInGroup("lineSide"))
				libLines.Add(line);
		}
		for (int i = libLines.Count - 1; i >= targetLibCount; i--)
		{
			RemoveChild(libLines[i]);
			libLines[i].QueueFree();
		}
		_currentLineCount = Mathf.Min(_currentLineCount, lineCount);
	}

	/// <summary> 立刻把弹夹补满到当前容量 </summary>
	private void FillLinesToCapacity()
	{
		while (_currentLineCount < lineCount)
			SpawnLine();
	}

	private void SpawnLine()
	{
			if (_currentLineCount >= lineCount) return;
			_currentLineCount++;
			var p = singleLineScene.Instantiate<Line>();
			p.parent = this;
			p.Position = GetPositionInCircle(_currentLineCount);
			CallDeferred(Node.MethodName.AddChild, p);
	}

	private void GiveVelocityAll(float delta)
	{
		int count = 0;
		int childCount = GetChildCount();
		for (int i = 0; i < childCount; i++)
		{
			var node = GetChild(i);
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
		if (GameManager.Instance.gameState != GameManager.GameState.GAMING) return false;
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
		bullet.throughLevel += 2*throughUgLevel;
		GlobalAudioPlayer.Instance.PlayAudio(attackSound);
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
			bullet.throughLevel += 2*throughUgLevel;
		}
		GlobalAudioPlayer.Instance.PlayAudio(attackSound);
	}

	private void ApplyShopUpgrades()
	{
		int ct = DataManager.Instance.GetUpgradeLevel("line_count");//
		int fr = DataManager.Instance.GetUpgradeLevel("fire_rate");//
		int ms = DataManager.Instance.GetUpgradeLevel("move_speed");//
		int atk = DataManager.Instance.GetUpgradeLevel("atk_hp");//
		int hp = DataManager.Instance.GetUpgradeLevel("max_hp");//
		int ls = DataManager.Instance.GetUpgradeLevel("lifesteal");//

		lineCount = 5 + ct;
		Line.AttackBonus = atk;
		fireRate += fr;
		GameManager.Instance.playerSpeed += ms * 20;
		HP += hp * 5;
		_hp = HP;

		if (ls > 0)
			SignalManager.Instance.OnEnemyDied += OnLifeSteal;

		// 升级后重新通知血条
		SignalManager.Instance.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged, 0, _hp, HP);
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
		SignalManager.Instance.OnPlayerDied -= OnLineDied;
		SignalManager.Instance.OnPlayerHealthChanged -= OnHealthChanged;
		SignalManager.Instance.OnBulletDestroyed -= SpawnLine;
		SignalManager.Instance.OnBulletDestroyed -= DownLaunchedBullet;
		base._ExitTree();
	}
}
