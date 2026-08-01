using Godot;
using System;

public partial class Line : Node2D
{
    [Export] private float bulletSpeed = 1000f;
    [Export] private int bulletAttackHP = 4;
    [Export] private int rotatingAttackHP = 3;
    [Export] public int 穿透等级 = 3;

    public static int AttackBonus;

    private bool rotating;
    private bool bulletInitialized;
    private bool isDestroyed;
    public Vector2 flySpeed;
    private float safeDestroyTimer;

    private int _left穿透;

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
            Position += flySpeed * (float)delta;

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
                ((Enemy)body).GotHurt(bulletAttackHP + AttackBonus);
                _left穿透--;
                if (_left穿透 <= 0)
                {
                    Destroy();
                }
            }
            else if (lineState == LineState.CORE && rotating)
            {
                ((Enemy)body).GotHurt((int)(rotatingAttackHP + AttackBonus * 0.2f));
            }
        }
    }

    private void Destroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        signalManager.EmitSignal("OnBulletDestroyed");
        QueueFree();
    }
}