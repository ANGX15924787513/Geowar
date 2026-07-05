using Godot;
using System;
using Godot.Collections;

public partial class PointPlayer : RigidBody2D
{
	[Export] private int pointCount;
	[Export] private float circleRadius;
	[Export] private PackedScene singlePointScene;
	[Export] private float pullStrenth = 2f;
	[Export] private float fireRotateSpeed = 3f;
	[Export] private int HP = 78;

	private int _currentPointCount;  // 运行时实时点数（发射 -1）
	private float _fireRotationTimer;
	private int _hp;
	GameManager gameManager;
	SignalManager signalManager;
	public override void _Ready()
	{
		_hp = HP;
		SummonPoints();
		gameManager = GetNode<GameManager>("/root/GameManager");
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		signalManager.OnCardOut += AddCamera;
		signalManager.OnBulletDestroyed += SpawnPoint;
	}

		public override void _ExitTree()
		{
			signalManager.OnCardOut -= AddCamera;
			signalManager.OnBulletDestroyed -= SpawnPoint;
		}

	public override void _Process(double delta)
	{
		GiveVelocityAll();
		if (_fireRotationTimer > 0f)
		{
			_fireRotationTimer -= (float)delta;
		}
		if (CanMove())
		{
			Move(delta);
		}

		if (canLaunchBullet())
		{
			LaunchBullet();
		}
	}

	private bool canLaunchBullet()
	{
		return
			gameManager.gameState == GameManager.GameState.GAMING &&
			Input.IsActionJustPressed("player_launch_bullet") &&
			_currentPointCount > 1
			;
	}

	private void LaunchBullet()
	{
		AngularVelocity = fireRotateSpeed;
		var bullet = GetClosestPoint(GetPoints(), GetTangentPoint());
		Vector2 bulletGlobalPos = bullet.GlobalPosition;
		bullet.GetParent().RemoveChild(bullet);
		_currentPointCount--;
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = bulletGlobalPos;
		bullet.LinearVelocity = GetGlobalMousePosition() - bullet.GlobalPosition;
		_fireRotationTimer = 0.5f;
	}

	private RigidBody2D GetClosestPoint(Array<RigidBody2D> points, Vector2 tangentPoint)
	{
		RigidBody2D closest = null;
		float minDistSq = float.MaxValue;

		foreach (RigidBody2D point in points)
		{
			float distSq = point.GlobalPosition.DistanceSquaredTo(tangentPoint);
			if (distSq < minDistSq)
			{
				minDistSq = distSq;
				closest = point;
			}
		}

		return closest;
	}

	private Array<RigidBody2D> GetPoints()
	{
		Array<RigidBody2D> rst = new Array<RigidBody2D>();
		foreach (Node node in GetChildren())
		{
			if (node is RigidBody2D)
			{
				rst.Add((RigidBody2D)node);
			}
		}

		return rst;
	}

	/// <summary>
	/// 从鼠标位置计算到圆圈的一个切点（顺时针方向）。
	/// 几何：圆心C、鼠标P、半径r，圆心角偏移 α = acos(r/d)。
	/// </summary>
	private Vector2 GetTangentPoint()
	{
		Vector2 mousePosition = GetGlobalMousePosition();
		Vector2 center = GlobalPosition;
		float distance = mousePosition.DistanceTo(center);

		// 圆心 → 鼠标 的单位方向
		Vector2 dirToMouse = (mousePosition - center).Normalized();

		// 鼠标在圆内/圆上没有真实切线，返回鼠标方向上的圆周点
		if (distance <= circleRadius)
			return center + dirToMouse * circleRadius;

		// 圆心角偏移 α = acos(r/d)
		float alpha = Mathf.Acos(circleRadius / distance);

		// 顺时针旋转得到切点（逆时针另一个切点用 +alpha）
		Vector2 tangentDir = dirToMouse.Rotated(-alpha);

		return center + tangentDir * circleRadius;
	}

	private Vector2 GetPositionInCircle(int index)
	{
		float angle = 2 * Single.Pi * index / _currentPointCount;
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
		_currentPointCount = pointCount;
		for (int i = 0; i < _currentPointCount; i++)
		{
			RigidBody2D point = (RigidBody2D)singlePointScene.Instantiate();
			point.Position = GetPositionInCircle(i);
			AddChild(point);
		}
	}

	/// <summary>
	/// 生成一个点补充到圆圈末尾。外部调用（如吃道具、击杀奖励等）。
	/// </summary>
	private void SpawnPoint()
	{
		RigidBody2D newPoint = (RigidBody2D)singlePointScene.Instantiate();
		newPoint.Position = GetPositionInCircle(_currentPointCount);
		_currentPointCount++;
		CallDeferred(Node.MethodName.AddChild, newPoint);
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

		// 发射旋转冷却中，不覆盖 AngularVelocity
		if (_fireRotationTimer > 0f)
			return;

		if (velocity.X != 0 && velocity.Y == 0)
		{
			AngularVelocity = velocity.X / gameManager.playerSpeed * gameManager.playerRotateSpeed;
		}
		else if (velocity.X == 0 && velocity.Y != 0)
		{
			AngularVelocity = velocity.Y / gameManager.playerSpeed * gameManager.playerRotateSpeed;
		}
	}

	private bool CanMove()
	{
		if (
			gameManager.gameState == GameManager.GameState.GAMING
		    )
		{
			return true;
		}
		return false;
	}

	private void AddCamera()
	{
		Camera2D camera = new Camera2D();
		camera.PositionSmoothingEnabled = true;
		AddChild(camera);
	}

	public void gotHurt(int damage)
	{
		if (damage <= 0)
		{
			return;
		}
		if (damage > _hp)
		{
			_hp = 0;
		}
		else
		{
			_hp -= damage;
		}
	}
}