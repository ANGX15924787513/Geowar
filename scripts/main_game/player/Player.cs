using Godot;

public partial class Player : RigidBody2D
{
	[Export] public int HP = 78;
	[Export] public float slowDownSmooth = 0.05f;
	[Export] public float rotateSmooth = 0.05f;
	[Export] private PackedScene hpBarScene;
	
	public static Player Instance;
	public bool isInvincible = false;

	private PlayerCamera playerCamera;
	
	public enum PlayerTypeEnum{POINT,LINE,POLYGON,NONE}
	public PlayerTypeEnum currentPlayerType;

	public GameManager gameManager;
	public SignalManager signalManager;
	public GlobalAudioPlayer globalAudioPlayer;
	public DataManager dataManager;

	protected int _hp;
	protected float _fireRotationTimer;
	private bool _isDead;
	protected float cs;
	
	private float hurtCooldown;
	
	public string os;

	public override void _Ready()
	{
		_hp = HP;
		gameManager = GetNode<GameManager>("/root/GameManager");
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
		dataManager =  GetNode<DataManager>("/root/DataManager");
		os = OS.GetName();
		signalManager.OnCardOut += AddCamera;
		signalManager.OnPlayerDied += OnDie;
		signalManager.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged,0,CurrentHP,HP);
		
		SummonHpBar();
		cs = 1 - dataManager.GetUpgradeLevel("camera_scale") * 0.1f;
		gameManager.playerSpeed = 500;
	}

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		_hp = 0;
		
		signalManager.OnCardOut -= AddCamera;
		signalManager.OnPlayerDied -= OnDie;
		Instance = null;
	}

	public override void _Process(double delta)
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return;
		if (_fireRotationTimer > 0f)
			_fireRotationTimer -= (float)delta;
		if (hurtCooldown > 0f)
			hurtCooldown -= (float)delta;
		if (!_isDead && _hp <= 0 && gameManager.gameState == GameManager.GameState.GAMING)
		{
			_isDead = true;
			signalManager.EmitSignal(SignalManager.SignalName.OnPlayerDied);
		}
	}

	public void ApplyMovement(float speed, float rotateSpeed)
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

	public void Hurt(int damage,Enemy from)
	{
		if (damage <= 0 || 
		    gameManager.gameState != GameManager.GameState.GAMING || 
		    hurtCooldown > 0 ||
		    isInvincible
		    ) return;
		hurtCooldown = 0.2f;
		playerCamera.Shake(20);
		AttackBack(from);
		_hp = Mathf.Max(0, _hp - damage);
		signalManager.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged, -damage, _hp, HP);
	}

	/// <summary>
	/// 敌人对玩家的击退
	/// </summary>
	/// <param name="from">攻击玩家的敌人</param>
	private void AttackBack(Enemy from)
	{
		const float atkBkStrength = 1400;
		Vector2 dir = (Position - from.Position).Normalized();
		LinearVelocity = atkBkStrength * dir;
	}

	public void GotHeal(int heal)
	{
		if (heal <= 0 || gameManager.gameState != GameManager.GameState.GAMING) return;
		// GD.Print($"治愈玩家 {heal} 点");
		_hp = Mathf.Min(_hp + heal, HP);
		signalManager.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged, heal, _hp, HP);
	}

	public int CurrentHP => _hp;

	private void AddCamera()
	{
		foreach (var child in GetChildren())
			if (child is Camera2D)
				return;

		PlayerCamera camera = new PlayerCamera();
		playerCamera = camera;
		camera.toZoom = new Vector2(cs, cs);
		camera.PositionSmoothingEnabled = true;
		AddChild(camera);
	}
	
	public void SummonHpBar()
	{
		var scene = hpBarScene.Instantiate();
		GetTree().CurrentScene.AddChild(scene);
	}

	public void OnDie()
	{
		GD.Print("游戏结束");
		gameManager.gameState = GameManager.GameState.GAME_OVER;
	}
	
	public void OnLifeSteal()
	{
		int ls = dataManager.GetUpgradeLevel("lifesteal");
		if (ls == 0 || _hp == HP) return;
		int heal = Mathf.Max(1, ls * 1);
		_hp = Mathf.Min(HP, _hp + heal);
		signalManager.EmitSignal(SignalManager.SignalName.OnPlayerHealthChanged, heal, _hp, HP);
	}
	
	public void OnHealthChanged(int change, int health, int maxHealth)
	{
		if (change < 0) globalAudioPlayer.PlayAudio(globalAudioPlayer.playerHurtSounds.PickRandom());
		else if(change > 0) globalAudioPlayer.PlayAudio(globalAudioPlayer.playerHealingSound);
	}
}
