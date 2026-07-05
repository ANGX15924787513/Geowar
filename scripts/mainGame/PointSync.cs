using Godot;

public partial class PointSync : RigidBody2D
{
	Sprite2D sprite;
	CollisionShape2D collisionShape2D;
	GpuParticles2D gpuParticles2D;
	[Export] private float 缩放关系 = 225f;
	[Export] private float size = 0.09f; 
	[Export] private float bulletSpeed = 4f;
	[Export] private int attackHP = 1;
	[Export] private Color bulletColor;
	bool bulletInitalized;
	private bool _isDestroyed;  // 防止重复触发 Destroy
	private Vector2 flySpeed;
	private SignalManager signalManager;

	public enum PointType
	{
		CIRCLE,
		BULLET
	}
	public PointType pointType;
	
	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		sprite = GetNode<Sprite2D>("Sprite2D");
		collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		gpuParticles2D = GetNode<GpuParticles2D>("GPUParticles2D");
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
				InitalizeBullet();
				bulletInitalized = true;
			}
			else
			{
				LinearVelocity = flySpeed;
			}
		}
	}

	private void InitalizeBullet()
	{
		Area2D area = new Area2D();
		area.AddChild(collisionShape2D.Duplicate());
		area.BodyEntered += OnBodyEntered;
		AddChild(area);
		LinearVelocity = bulletSpeed * LinearVelocity;
		flySpeed = LinearVelocity;
		((PointParticle)GetNode<GpuParticles2D>("GPUParticles2D")).color = bulletColor;
	}

	private void OnBodyEntered(Node2D body)
	{
		GD.Print($"子弹创到:{body.Name}");
		if (body.IsInGroup("wall"))
		{
			Destroy();
		}else if (body.IsInGroup("enemy"))
		{
			((Enemy)body).Health -= attackHP;
			Destroy();
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
