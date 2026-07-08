using Godot;

public partial class Player : RigidBody2D
{
	[Export] public int HP = 78;
	[Export] public float slowDownSmooth = 0.05f;
	[Export] public float rotateSmooth = 0.05f;

	public GameManager gameManager;
	public SignalManager signalManager;
	public GlobalAudioPlayer globalAudioPlayer;

	protected int _hp;
	protected float _fireRotationTimer;
	private bool _isDead;

	public override void _Ready()
	{
		_hp = HP;
		gameManager = GetNode<GameManager>("/root/GameManager");
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
		signalManager.OnCardOut += AddCamera;
		signalManager.OnPlayerDied += OnDie;
		signalManager.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged,0,CurrentHP,HP);
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
	
	protected void ApplyMovement(float speed, float rotateSpeed)
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

		if (_fireRotationTimer > 0f)
			return;

		if (velocity.X != 0 && velocity.Y == 0)
			AngularVelocity = (float)(velocity.X / speed * rotateSpeed * Engine.TimeScale);
		else if (velocity.X == 0 && velocity.Y != 0)
			AngularVelocity = (float)(velocity.Y / speed * rotateSpeed * Engine.TimeScale);
	}

	protected bool CanMove() => gameManager.gameState == GameManager.GameState.GAMING;

	/// <summary> 游戏结束或不可移动时平滑减速直到停止 </summary>
	protected void Decelerate(double delta)
	{
		LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, 2f * (float)delta);
		AngularVelocity = Mathf.Lerp(AngularVelocity, 0f, 2f * (float)delta);
	}

	public void GotHurt(int damage)
	{
		if (damage <= 0 || gameManager.gameState != GameManager.GameState.GAMING) return;
		GD.Print($"PlayerGotHurt:{damage}");
		_hp = Mathf.Max(0, _hp - damage);
		signalManager.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged, damage, _hp, HP);
	}

	public int CurrentHP => _hp;

	private void AddCamera()
	{
		foreach (var child in GetChildren())
			if (child is Camera2D)
				return;

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
