using Godot;

public partial class Enemy : RigidBody2D
{
    public enum EnemyTypeEnum
    {
        NORMAL,
        SUPER,
        EXTREME
    }

    public EnemyTypeEnum enemyType;
    [Export] public int MaxHealth { get; set; } = 10;
    [Export] public float Speed { get; set; } = 150;
    [Export] public int Damage { get; set; } = 3;

    public int health { get; set; }
    public bool IsDead => health <= 0;
    protected bool _isDying;
    protected Node2D _targetPlayer;
    public GameManager gameManager;
    public SignalManager signalManager;
    GlobalAudioPlayer globalAudioPlayer;
    DataManager dataManager;

    [Export] private float _reactionTime = 0.8f;   // 反应间隔基准值
    [Export] private float _steeringForce = 2f;   // 转向力度基准值
    
    [Export] private AudioStream hurtSound;

    // 每实例随机值，避免所有怪物路径相同
    private float _randReactionTime;
    private float _randSteeringForce;
    private float _speedMultiplier; // Speed 的随机倍率，实时速 = Speed × 此值
    private Vector2 _randTargetOffset;

    private Vector2 _delayedTarget;
    private bool _delayedTargetReady;
    private float _timeSinceReaction;

    public override void _Ready()
    {
        gameManager = GetNode<GameManager>("/root/GameManager");
        signalManager =  GetNode<SignalManager>("/root/SignalManager");
        globalAudioPlayer =  GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
        dataManager = GetNode<DataManager>("/root/DataManager");
        _targetPlayer = GetTree().GetFirstNodeInGroup("player") as Node2D;
        
        // 随机初始化：每个怪物的反应速度、惯性、移速、目标偏移都略有不同
        _randReactionTime = _reactionTime * (float)GD.RandRange(0.6, 1.5);
        _randSteeringForce = _steeringForce * (float)GD.RandRange(0.5, 1.8);
        _speedMultiplier = (float)GD.RandRange(0.5, 1.5);
        _randTargetOffset = new Vector2(
            GD.RandRange(-80, 80),
            GD.RandRange(-80, 80)
        );
        
        MaxHealth += (int)(dataManager.GetUpgradeLevel("max_hp") * 0.3);
        health = MaxHealth;

        _delayedTarget = GlobalPosition;
        _delayedTargetReady = true;
    }

    public override void _Process(double delta)
    {
        if (health <= 0 && !_isDying)
        {
            _isDying = true; DiedHandler();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (gameManager.gameState == GameManager.GameState.GAMING)
        {
                MoveToPlayer((float)delta);
        }
        else
        {
            LinearVelocity = Vector2.Zero;
        }
    }

    private void MoveToPlayer(float delta)
    {
        if (_targetPlayer == null)
        {
            // 玩家不存在时停止移动，避免惯性滑行
            LinearVelocity = Vector2.Zero;
            return;
        }
        _timeSinceReaction += delta;

        // 每个怪物的反应节奏不同，锁定时加入随机偏移
        if (_timeSinceReaction >= _randReactionTime)
        {
            _timeSinceReaction = 0f;
            _delayedTarget = _targetPlayer.GlobalPosition + _randTargetOffset;
        }

        // 目标方向与距离
        Vector2 toTarget = _delayedTarget - GlobalPosition;

        // 足够近就平滑停下
        // if (distSq < 1f)
        // {
        //     LinearVelocity = LinearVelocity.Lerp(Vector2.Zero, _randSteeringForce * dt);
        //     return;
        // }

        // 惯性转向：每个怪物的速度和转向力度都不同
        Vector2 desiredVelocity = toTarget.Normalized() * Speed * _speedMultiplier;
        LinearVelocity = LinearVelocity.Lerp(desiredVelocity, _randSteeringForce * delta);
    }

    public void GotHurt(int damage,Node2D from,bool isAtkBk = true)
    {
        if (damage <= 0) return;
        health = Mathf.Max(0, health - damage);
        globalAudioPlayer.PlayAudio(hurtSound,globalAudioPlayer.PlayerAttackVolume);
        if(isAtkBk && from != null) AttackBack(from);
        signalManager.EmitSignal(SignalManager.SignalName.OnEnemyGotHurt, damage,GlobalPosition);
    }
    
    private void AttackBack(Node2D from)
    {
        const float atkBkStrength = 900;
        Vector2 dir = (Position - from.Position).Normalized();
        LinearVelocity = atkBkStrength * dir;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_isDying) return;
        if (body is Player player)
            player.GotHurt(Damage + dataManager.GetUpgradeLevel("atk_hp") / 2,this);
    }
    
    protected void DiedHandler()
    {
        if (gameManager.gameState != GameManager.GameState.GAMING) return;
        GetNode<Area2D>("Area2D").CollisionMask &= ~(uint)(1 << (3 - 1));
        RemoveFromGroup("enemy");
        CollisionLayer &= ~(uint)(1 << (3 - 1));
        Tween hideTn = GetTree().CreateTween();
        hideTn.TweenProperty(this, "modulate", new Color(Colors.White, 0), 0.2f);
        hideTn.Finished += () =>
        {
            signalManager.EmitSignal(SignalManager.SignalName.OnEnemyDied);
            signalManager.EmitSignal(SignalManager.SignalName.RequestCollectionSpawn, GlobalPosition,(int)enemyType);
            QueueFree();
        };
    }

    public void SetScale(float sc)
    {
        foreach (Node node in GetChildren())
        {
            if (node is not Node2D ci) continue;
            ci.Scale *= sc;
        }
    }
}