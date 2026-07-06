using Godot;

public partial class PointSync : RigidBody2D
{
	Sprite2D sprite;
	CollisionShape2D collisionShape2D;
	GpuParticles2D gpuParticles2D;
	[Export] private float 缩放关系 = 225f;
	[Export] private float size = 0.09f; 
	[Export] private float bulletSpeed = 1000f;
	[Export] private int attackHP = 1;
	[Export] private Color bulletColor;
	bool bulletInitalized;
	private bool _isDestroyed;  // 防止重复触发 Destroy
	private Vector2 flySpeed;
	private SignalManager signalManager;
	private GameManager gameManager;

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
			else
			{
				LinearVelocity = flySpeed;
			}
		}
	}

	private void InitializeBullet()
	{
		// Area2D area = new Area2D();
		// area.AddChild(collisionShape2D.Duplicate());
		// area.BodyEntered += OnBodyEntered;
		// AddChild(area);
		LinearVelocity = bulletSpeed * LinearVelocity.Normalized();
		flySpeed = LinearVelocity;
		GetNode<PointParticle>("Sprite2D/GPUParticles2D").color = bulletColor;
		GD.Print("子弹初始化");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return;
		GD.Print($"子弹创到:{body.Name}");
		if (body.IsInGroup("wall"))
		{
			Destroy();
		}else if (body.IsInGroup("enemy"))
		{
			((Enemy)body).GotHurt(attackHP);
			if (pointType == PointType.BULLET)
			{
				Destroy();
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
