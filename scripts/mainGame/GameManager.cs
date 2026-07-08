using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public enum GameState
    {
        MAIN_MENU,
        CHOOSE_CARD,
        GAMING,
        GAME_OVER,
        GAME_PAUSED
    }
    [Export] public GameState gameState;
    public bool charChose { get; set; } = false;
    public float playerSpeed = 500f;
    public float playerRotateSpeed = 1f;
    public float playerRebornTime = 0.2f;
    public static GameManager Instance { get; private set; }

    // ---- 商店升级 (6项) ----
    public int pointCountUpgrade;
    public int fireRateUpgrade;
    public int moveSpeedUpgrade;
    public int atkHpUpgrade;
    public int maxHpUpgrade;
    public int lifestealUpgrade;
    
    public enum PlayerType
    {
        POINT,
        LINE,
        POLYGON
    }
    public PlayerType playerType;
    public int playerTypeCount = 3;
    
    public List<PackedScene> playerScene;
    
    public SignalManager signalManager;

    public override void _Ready()
    {
        Instance = this;
        playerScene =
        [
            GD.Load<PackedScene>("res://scenes/main/point/pointPlayer.tscn"), //Point
            GD.Load<PackedScene>("res://scenes/main/line/linePlayer.tscn"), //Line
            GD.Load<PackedScene>("res://scenes/main/polygon/polygonPlayer.tscn") //Polygon
        ];
        signalManager = GetNode<SignalManager>("/root/SignalManager");
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.F11))
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);
        }
    }

    public void SummonPlayer(SceneTree tree)
    {
        if (playerScene.Count == 0)
            _Ready();

        var player = (Node2D)playerScene[(int)playerType].Instantiate();
        player.GlobalPosition = GetScreenCenter();
        tree.CurrentScene.AddChild(player);
        gameState = GameState.GAMING;
        charChose = false;
        signalManager.EmitSignal(SignalManager.SignalName.OnCardOut);
    }
    
    // 获取屏幕中央坐标（相对于当前视口）
    public Vector2 GetScreenCenter()
    {
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        return viewportSize / 2;
    }
}