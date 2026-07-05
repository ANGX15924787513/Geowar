using Godot;

public partial class Player : RigidBody2D
{
	[Export] public int HP = 78;

	public GameManager gameManager;
	public SignalManager signalManager;

	protected int _hp;
	protected float _fireRotationTimer;
	private bool _isDead;

	public override void _Ready()
	{
		_hp = HP;
		gameManager = GetNode<GameManager>("/root/GameManager");
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		signalManager.OnCardOut += AddCamera;
		signalManager.OnPlayerDied += OnDie;
	}

	public override void _ExitTree()
	{
		signalManager.OnCardOut -= AddCamera;
		signalManager.OnPlayerDied -= OnDie;
	}

	public override void _Process(double delta)
	{
		if (_fireRotationTimer > 0f)
			_fireRotationTimer -= (float)delta;
		if (!_isDead && _hp <= 0 && gameManager.gameState == GameManager.GameState.GAMING)
		{
			_isDead = true;
			signalManager.EmitSignal(SignalManager.SignalName.OnPlayerDied);
		}
	}

	/// <summary> 通用移动：线速度 + 角速度，旋转冷却中不覆盖角速度 </summary>
	protected void ApplyMovement(float speed, float rotateSpeed)
	{
		Vector2 velocity = speed * Input.GetVector(
			"player_left", "player_right", "player_up", "player_down");
		LinearVelocity = velocity * (float)Engine.TimeScale;

		if (_fireRotationTimer > 0f)
			return;

		if (velocity.X != 0 && velocity.Y == 0)
			AngularVelocity = (float)(velocity.X / speed * rotateSpeed * Engine.TimeScale);
		else if (velocity.X == 0 && velocity.Y != 0)
			AngularVelocity = (float)(velocity.Y / speed * rotateSpeed * Engine.TimeScale);
	}

	protected bool CanMove() => gameManager.gameState == GameManager.GameState.GAMING;

	public void GotHurt(int damage)
	{
		GD.Print($"GotHurt:{damage}");
		if (damage <= 0) return;
		_hp = Mathf.Max(0, _hp - damage);
	}

	public int CurrentHP => _hp;

	private void AddCamera()
	{
		Camera2D camera = new Camera2D();
		camera.PositionSmoothingEnabled = true;
		AddChild(camera);
	}

	public void OnDie()
	{
		GD.Print("Game Over");
		gameManager.gameState = GameManager.GameState.GAME_OVER;
	}
}
