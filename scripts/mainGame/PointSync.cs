using Godot;

public partial class PointSync : RigidBody2D
{
	Sprite2D sprite;
	CollisionShape2D collisionShape2D;
	GpuParticles2D gpuParticles2D;
	[Export] private float 缩放关系;
	[Export] private float size = 0.09f;
	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite2D");
		collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		gpuParticles2D = GetNode<GpuParticles2D>("GPUParticles2D");
		SyncScale();
	}

	public override void _Process(double delta)
	{
		SyncScale();
	}

	private void SyncScale()
	{
		Vector2 vector2Scale = sprite.Scale with { X = size ,Y = size };
		sprite.Scale = vector2Scale;
		((ParticleProcessMaterial)gpuParticles2D.ProcessMaterial).Scale = vector2Scale;
		((CircleShape2D)collisionShape2D.Shape).Radius = 缩放关系 * sprite.Scale.X;
	}
}
