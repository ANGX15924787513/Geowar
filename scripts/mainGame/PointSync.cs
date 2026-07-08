using Godot;

public partial class PointSync : RigidBody2D
{
	Sprite2D sprite;
	CollisionShape2D collisionShape2D;
	GpuParticles2D gpuParticles2D;
	[Export] private float 缩放关系 = 225f;
	[Export] private float size = 0.09f; 
	[Export] private float bulletSpeed = 1000f;
	[Export] private int circleAttackHP = 1;
	[Export] private int bulletAttackHP = 5;
	public static int AttackBonus;
	[Export] private Color bulletColor;
	bool bulletInitalized;
	private bool _isDestroyed;  // 防止重复触发 Destroy
	private Vector2 flySpeed;
	private Node2D _homingTarget;
	[Export] private float homingStrength = 3f; // 追踪转向力度
	private SignalManager signalManager;
	private GameManager gameManager;
	
	
	[Export] private bool debugFollowMode;
	

	public enum PointType
	{
		CIRCLE,
		BULLET
	}
	[Export] public PointType pointType;
	
	public override void _Ready()
	{
		gameManager =  GetNode<GameManager>("/root/GameManager");
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		sprite = GetNode<Sprite2D>("Sprite2D");
		collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		gpuParticles2D = GetNode<GpuParticles2D>("Sprite2D/GPUParticles2D");
		SyncScale();
	}

	public override void _Process(double delta)
	{
		SyncScale();
		UpdatePointType();
		if (pointType == PointType.BULLET)
		{
			if (!bulletInitalized)
			{
				InitializeBullet();
				bulletInitalized = true;
			}
			else if (debugFollowMode)
			{
				HomeToNearestEnemy((float)delta);
			}
			else
			{
				LinearVelocity = flySpeed;
			}
		}
	}

	private void HomeToNearestEnemy(float delta)
	{
		// 目标挂了就不再追
		if (_homingTarget == null || !IsInstanceValid(_homingTarget))
		{
			LinearVelocity = flySpeed;
			return;
		}

		Vector2 to = _homingTarget.GlobalPosition - GlobalPosition;
		LinearVelocity = to.Normalized() * bulletSpeed;
	}

	private void InitializeBullet()
	{
		LinearVelocity = bulletSpeed * LinearVelocity.Normalized();
		flySpeed = LinearVelocity;
		GetNode<PointParticle>("Sprite2D/GPUParticles2D").color = bulletColor;

		// 锁定发射时最近的敌人
		if (debugFollowMode)
		{
			float best = float.MaxValue;
			foreach (var node in GetTree().GetNodesInGroup("enemy"))
			{
				if (node is not Node2D body || !IsInstanceValid(body)) continue;
				float d = GlobalPosition.DistanceSquaredTo(body.GlobalPosition);
				if (d < best) { best = d; _homingTarget = body; }
			}
		}

		GD.Print("子弹初始化");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return;
		if (body.IsInGroup("wall") && pointType == PointType.BULLET)
		{
			GD.Print($"子弹创到:{body.Name}");
			Destroy();
		}else if (body.IsInGroup("enemy"))
		{
			GD.Print($"子弹创到:{body.Name}");
			if (pointType == PointType.BULLET)
			{
				((Enemy)body).GotHurt(bulletAttackHP + AttackBonus);
				Destroy();
			}else if (pointType == PointType.CIRCLE)
			{
				((Enemy)body).GotHurt(circleAttackHP + AttackBonus);
			}
		}
		void Destroy()
		{
			if (_isDestroyed) return;
			_isDestroyed = true;
			signalManager.EmitSignal("OnBulletDestroyed");
			QueueFree();
		}
	}

	private void SyncScale()
	{
		Vector2 vector2Scale = sprite.Scale with { X = size ,Y = size };
		sprite.Scale = vector2Scale;
		((ParticleProcessMaterial)gpuParticles2D.ProcessMaterial).Scale = vector2Scale;
		((CircleShape2D)collisionShape2D.Shape).Radius = 缩放关系 * sprite.Scale.X;
	}

	private void UpdatePointType()
	{
		pointType = GetParent().IsInGroup("pointController") ? PointType.CIRCLE : PointType.BULLET;
	}
}
