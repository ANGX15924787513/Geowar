using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    public enum GameState
    {
        MAIN_MENU,
        CHOOSE_CARD,
        GAMING,
        GAME_OVER,
        GAME_PAUSED,
        STARTING
    }
    [Export] public GameState gameState = GameState.STARTING;
    
    [Signal] public delegate void OnGameStateChangeEventHandler(GameState newState);
    
    // --- 难度处理 ---
    public readonly float LINE_RUSH = 0.3f;
    public readonly float SUPER_ENY = 0.2f;
    public readonly float SUSUPER_ENY = 0.5f;

    public bool glowDisabled;
    
    public bool charChose { get; set; } = false;
    public float playerSpeed = 500f;
    public float playerRotateSpeed = 1f;
    public float playerRebornTime = 0.2f;
    public readonly Vector2 gameAreaX = new(-1650f,2550f);
    public readonly Vector2 gameAreaY = new(-1000f,1086f);
    public static GameManager Instance { get; private set; }

    public float difficulty;
    //在商店中更新

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
    public DataManager dataManager;

    private bool hideMouse;
    private GameState just;
    
    private static readonly Vector2I originWindowSize = new(1880, 1080);

    public override void _Ready()
    {
        ScreenInit();
        GD.Print($"当前平台: {OS.GetName()}");
        just = gameState;
        GD.Print($"当前游戏状态: {gameState}");
        Instance = this;
        playerScene =
        [
            GD.Load<PackedScene>("res://scenes/main/point/pointPlayer.tscn"), //Point
            GD.Load<PackedScene>("res://scenes/main/line/linePlayer.tscn"), //Line
            GD.Load<PackedScene>("res://scenes/main/polygon/polygonPlayer.tscn") //Polygon
        ];
        dataManager = GetNode<DataManager>("/root/DataManager");
        signalManager = GetNode<SignalManager>("/root/SignalManager");

        hideMouse = dataManager.GetBoolConfig("hide_mouse_playing");
        glowDisabled = dataManager.GetBoolConfig("glow_disabled");
    }

    private void ScreenInit()
    {
        DisplayServer.WindowSetSize(originWindowSize);
        DisplayServer.WindowSetPosition(DisplayServer.ScreenGetSize() / 2 - DisplayServer.WindowGetSize() / 2);
        #if TOOLS
            GetTree().Root.Unresizable = false;
        #endif
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.F11))
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen?DisplayServer.WindowMode.Windowed:DisplayServer.WindowMode.Fullscreen);
        }

        if (gameState == GameState.GAMING && hideMouse)
        {
            Input.SetMouseMode(Input.MouseModeEnum.Hidden);
        }
        else
        {
            Input.SetMouseMode(Input.MouseModeEnum.Visible);
        }

        if (just != gameState)
        {
            GD.Print($"游戏状态切换: {gameState}");
            EmitSignal(SignalName.OnGameStateChange, (int)gameState);
            just = gameState;   
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

    public void RefreshGlow()
    {
        GetNode<WorldEnvironment>("/root/mainMenu/WorldEnvironment").Environment.GlowEnabled = !glowDisabled;
    }
}