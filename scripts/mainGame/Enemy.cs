using Godot;
using System;

public partial class Enemy : RigidBody2D
{
	[Export] public int MaxHealth { get; set; } = 10;
	[Export] public float Speed { get; set; } = 100;
	[Export] public int Damage { get; set; } = 6;
    
	public int Health { get; set; }
	public bool IsDead => Health <= 0;
	protected Node2D _targetPlayer;
	GameManager gameManager;
	
	[Export] private float _trackingResponse = 3f; // 延迟追赶速度，越小越滞后
	private Vector2 _delayedTarget;
	private bool _delayedTargetReady;
	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		_targetPlayer = GetTree().GetFirstNodeInGroup("player") as Node2D;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (gameManager.gameState == GameManager.GameState.GAMING)
		{
			MoveToPlayer();
		}
		else
		{
			LinearVelocity = Vector2.Zero;
		}
	}

	/// <summary>
	/// 平滑延迟追踪玩家。_delayedTarget 缓慢追赶玩家位置，敌人再追 _delayedTarget，
	/// 产生"拖尾跟随"的顺滑效果。
	/// </summary>
	private void MoveToPlayer()
	{
		if (_targetPlayer == null) return;

		float dt = (float)GetProcessDeltaTime();

		// 首次初始化延迟目标为自身位置，避免闪现到原点
		if (!_delayedTargetReady)
		{
			_delayedTarget = GlobalPosition;
			_delayedTargetReady = true;
		}

		// 延迟目标向玩家位置平滑靠拢
		_delayedTarget = _delayedTarget.Lerp(_targetPlayer.GlobalPosition, _trackingResponse * dt);

		// 敌人向延迟目标移动
		Vector2 toTarget = _delayedTarget - GlobalPosition;
		float distSq = toTarget.LengthSquared();

		if (distSq < 4f) // 足够近就停，避免抖动
		{
			LinearVelocity = Vector2.Zero;
			return;
		}

		LinearVelocity = toTarget.Normalized() * Speed * (float)Engine.TimeScale;
	}

	public void GotHurt(int damage)
	{
		if (damage <= 0) return;
		Health = Mathf.Max(0, Health - damage);
	}
}
