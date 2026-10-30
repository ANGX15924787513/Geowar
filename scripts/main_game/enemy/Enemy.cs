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
    [Export] private float _reactionTime = 0.8f;   // 反应间隔基准值
    [Export] private float _steeringForce = 2f;   // 转向力度基准值
    
    [Export] private AudioStream hurtSound;

    // 每实例随机值，避免所有怪物路径相同
    private float _randReactionTime;
    private float _randSteeringForce;
    private float _speedMultiplier; // Speed 的随机倍率，实时速 = Speed × 此值
    private Vector2 _randTargetOffset;

    //灼烧
    private bool isFiring;
    protected float _fireTimer;
    public float firedCooldown = 0.3f;
    private int _fireDamage;
    private Color catheModulate;

    private Vector2 _delayedTarget;
    private bool _delayedTargetReady;
    private float _timeSinceReaction;

    public override void _Ready()
    {
        _targetPlayer = GetTree().GetFirstNodeInGroup("player") as Node2D;
        
        // 随机初始化：每个怪物的反应速度、惯性、移速、目标偏移都略有不同
        _randReactionTime = _reactionTime * (float)GD.RandRange(0.6, 1.5);
        _randSteeringForce = _steeringForce * (float)GD.RandRange(0.5, 1.8);
        _speedMultiplier = (float)GD.RandRange(0.5, 1.5);
        _randTargetOffset = new Vector2(
            GD.RandRange(-80, 80),
            GD.RandRange(-80, 80)
        );
        
        MaxHealth += (int)(DataManager.Instance.GetUpgradeLevel("max_hp") * 0.3);
        health = MaxHealth;

        _delayedTarget = GlobalPosition;
        _delayedTargetReady = true;
    }

    public override void _Process(double delta)
    {
        if (GameManager.Instance.gameState == GameManager.GameState.GAMING && _fireTimer > 0)
        {
            _fireTimer -= (float)delta;
        }else if (GameManager.Instance.gameState == GameManager.GameState.GAMING && _fireTimer < 0)
        {
            Hurt(_fireDamage,null,false,GlobalAudioPlayer.Instance.onFireSound);
            _fireTimer = firedCooldown;
        }
        if (health <= 0 && !_isDying)
        {
            _isDying = true; DiedHandler();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameManager.Instance.gameState == GameManager.GameState.GAMING)
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
        if (!IsInstanceValid(_targetPlayer))
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

    public void Hurt(int damage,Node2D from,bool isAtkBk = true,AudioStream attackSound = null)
    {
        if (damage <= 0) return;
        health = Mathf.Max(0, health - damage);
        GlobalAudioPlayer.Instance.PlayAudio(attackSound ?? hurtSound,GlobalAudioPlayer.Instance.PlayerAttackVolume);
        if(isAtkBk && from != null) AttackBack(from);
        SignalManager.Instance.EmitSignal(SignalManager.SignalName.OnEnemyGotHurt, damage,GlobalPosition);
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
            player.Hurt(Damage + DataManager.Instance.GetUpgradeLevel("atk_hp") / 2,this);
    }
    
    protected void DiedHandler()
    {
        if (GameManager.Instance.gameState != GameManager.GameState.GAMING) return;
        GetNode<Area2D>("Area2D").CollisionMask &= ~(uint)(1 << (3 - 1));
        RemoveFromGroup("enemy");
        CollisionLayer &= ~(uint)(1 << (3 - 1));
        Tween hideTn = GetTree().CreateTween();
        hideTn.TweenProperty(this, "modulate", new Color(Colors.White, 0), 0.2f);
        hideTn.Finished += () =>
        {
            SignalManager.Instance.EmitSignal(SignalManager.SignalName.OnEnemyDied);
            SignalManager.Instance.EmitSignal(SignalManager.SignalName.RequestCollectionSpawn, GlobalPosition,(int)enemyType);
            QueueFree();
        };
    }

    public void LetFired(int fireDamage)
    {
        _fireTimer = firedCooldown;
        isFiring = true;
        _fireDamage = fireDamage;
        GetTree().CreateTimer(5).Timeout += LetUnfired;
        catheModulate = Modulate;
        Modulate = Modulate.Lerp(Colors.Red, 0.2f);
    }
    
    public void LetUnfired()
    {
        _fireTimer = 0;
        isFiring = false;
        Modulate = catheModulate;
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