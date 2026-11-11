using Godot;

public partial class Line : Node2D
{
    [Export] private float bulletSpeed = 1000f;
    [Export] private int bulletAttackHP = 4;
    [Export] private int rotatingAttackHP = 3;
    [Export] public int throughLevel = 3;
    [Export] private CompressedTexture2D lightTex;
    // [Export] private PackedScene fireScene;

    public static int AttackBonus;

    private bool rotating;
    private bool bulletInitialized;
    private bool isDestroyed;
    public Vector2 flySpeed;
    private float safeDestroyTimer;
    
    private bool canFiring;
    private CollisionShape2D originCollision;
    private int fireDamage;
    SceneTreeTimer desTimer;

    private int _leftThrough;

    [Export] private GpuParticles2D fireParticle;

    public enum LineState { CORE, BULLET }

    [Export] public LineState lineState;

    public override void _Ready()
    {
        originCollision = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        throughLevel += DataManager.Instance.GetUpgradeLevel("through") * 3;
        
        fireDamage = 2 * DataManager.Instance.GetUpgradeLevel("line_fire");
        canFireEnemy = fireDamage == 0;
    }

    public override void _Process(double delta)
    {
        lineState = GetParent().IsInGroup("player") ? LineState.CORE : LineState.BULLET;

        if (lineState == LineState.BULLET && !bulletInitialized)
        {
            InitializeBullet();
            bulletInitialized = true;
        }

        // 改为直接修改位置
        if (lineState == LineState.BULLET && bulletInitialized)
        {
            Position += flySpeed * (float)delta;
        }

        if (safeDestroyTimer > 0)
            safeDestroyTimer -= (float)delta;
    }

    private void InitializeBullet()
    {
        _leftThrough = throughLevel;
        // 使用当前 flySpeed 的方向作为飞行方向（发射时由外部预先设置）
        Vector2 dir = flySpeed.Normalized();
        // flySpeed 重新赋值为最终速度（矢量，单位/秒）
        flySpeed = dir * bulletSpeed * 2f;
        Rotation = dir.Angle() - Mathf.Pi/2;
        safeDestroyTimer = 0.2f;
        canFiring = DataManager.Instance.GetUpgradeLevel("line_fire") >= 1;
        desTimer = GetTree().CreateTimer(5);
        if (canFiring)
        {
            fireParticle.Visible = true;
            fireParticle.ProcessMode = ProcessModeEnum.Inherit;
        }
    }

    private bool canFireEnemy;

    private void OnBodyEntered(Node2D body)
    {
        if (GameManager.Instance.gameState != GameManager.GameState.GAMING) return;

        if (body.IsInGroup("wall") && lineState == LineState.BULLET)
        {
            if (safeDestroyTimer > 0) return;
            Destroy();
        }
        else if (body.IsInGroup("enemy"))
        {
            if (lineState == LineState.BULLET)
            {
                if(canFireEnemy) ((Enemy)body).LetFired(fireDamage);
                ((Enemy)body).Hurt(bulletAttackHP + AttackBonus,this);
                _leftThrough--;
                if (_leftThrough <= 0)
                {
                    Destroy();
                }
            }
            TayTool.CollideLight(GlobalPosition,Colors.White, GetTree(),4);
        }
    }

    private void Destroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        SignalManager.Instance.EmitSignal("OnBulletDestroyed");
        if (IsInstanceValid(desTimer))
        {
            desTimer.Timeout += QueueFree;
            return;
        }
        
        QueueFree();
    }
}