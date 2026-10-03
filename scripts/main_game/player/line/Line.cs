using Godot;

public partial class Line : Node2D
{
    [Export] private float bulletSpeed = 1000f;
    [Export] private int bulletAttackHP = 4;
    [Export] private int rotatingAttackHP = 3;
    [Export] public int 穿透等级 = 3;
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

    private int _left穿透;

    [Export] private GpuParticles2D fireParticle;

    private SignalManager signalManager;
    private GameManager gameManager;
    private DataManager dataManager;

    public enum LineState { CORE, BULLET }

    [Export] public LineState lineState;

    public override void _Ready()
    {
        gameManager = GetNode<GameManager>("/root/GameManager");
        signalManager = GetNode<SignalManager>("/root/SignalManager");
        dataManager = GetNode<DataManager>("/root/DataManager");
        originCollision = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        穿透等级 += dataManager.GetUpgradeLevel("through") * 3;
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
        _left穿透 = 穿透等级;
        // 使用当前 flySpeed 的方向作为飞行方向（发射时由外部预先设置）
        Vector2 dir = flySpeed.Normalized();
        // flySpeed 重新赋值为最终速度（矢量，单位/秒）
        flySpeed = dir * bulletSpeed * 2f;
        Rotation = dir.Angle() - Mathf.Pi/2;
        safeDestroyTimer = 0.2f;
        canFiring = dataManager.GetUpgradeLevel("line_fire") >= 1;
        desTimer = GetTree().CreateTimer(5);
        if (canFiring)
        {
            fireParticle.Visible = true;
            fireParticle.ProcessMode = ProcessModeEnum.Inherit;
        }
    }
    
    private void FireEnemy(Enemy enemy)
    {
        fireDamage = 2 * dataManager.GetUpgradeLevel("line_fire");
        enemy.LetFired(fireDamage);
        
    }

    private void OnBodyEntered(Node2D body)
    {
        if (gameManager.gameState != GameManager.GameState.GAMING) return;

        if (body.IsInGroup("wall") && lineState == LineState.BULLET)
        {
            if (safeDestroyTimer > 0) return;
            Destroy();
        }
        else if (body.IsInGroup("enemy"))
        {
            if (lineState == LineState.BULLET)
            {
                FireEnemy((Enemy)body);
                ((Enemy)body).Hurt(bulletAttackHP + AttackBonus,this);
                _left穿透--;
                if (_left穿透 <= 0)
                {
                    Destroy();
                }
            }
            for (int i = 0; i < 10; i++)
            {
                CollideLight((GlobalPosition + body.GlobalPosition)/2,Colors.WhiteSmoke);
            }
        }
    }
    
    private void CollideLight(Vector2 position,Color color)
    {
        float duration = 0.8f;
        var light = new PointLight2D();
        light.Texture = lightTex;
        light.TextureScale = 0.1f;
        light.Color = color;
        light.Energy = 2;
        light.Position = position + new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f) * 40;
        GetTree().CurrentScene.AddChild(light);
        Tween tw = GetTree().CreateTween();
        Tween tw2 = GetTree().CreateTween();
        Tween tw3 = GetTree().CreateTween();
        tw.TweenProperty(light,"texture_scale",0.02f,duration);
        tw2.TweenProperty(light,"position",light.Position + new Vector2(GD.Randf() - 0.5f, GD.Randf() - 0.5f) * 300,duration);
        tw3.TweenProperty(light,"modulate",new Color(1, 1, 1, 0.5f), duration);
        GetTree().CreateTimer(duration).Timeout += light.QueueFree;
    }

    private void Destroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        signalManager.EmitSignal("OnBulletDestroyed");
        if (IsInstanceValid(desTimer))
        {
            desTimer.Timeout += QueueFree;
            return;
        }
        
        QueueFree();
    }
}