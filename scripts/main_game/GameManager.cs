using Godot;
using System.Collections.Generic;
using System.Linq;

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
    [ExportGroup("Player Scenes")]
    [Export] private PackedScene pointPlayerScene;
    [Export] private PackedScene linePlayerScene;
    [Export] private PackedScene polygonPlayerScene;
    
    [Signal] public delegate void OnGameStateChangeEventHandler(GameState newState);
    
    // --- 难度处理 ---
    public readonly float LINE_RUSH = 0.3f;
    public readonly float SUPER_ENY = 0.2f;
    public readonly float SUSUPER_ENY = 0.5f;

    public bool glowDisabled;
    
    public bool charChose { get; set; }
    public float playerSpeed = 500f;
    public float playerRotateSpeed = 1f;
    public float playerRebornTime = 0.2f;
    public readonly Vector2 gameAreaX = new(-1650f,2550f);
    public readonly Vector2 gameAreaY = new(-1000f,1086f);
    public readonly Vector2 gameAreaCentre = new(450,43);
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

    private bool hideMouse;
    private GameState just;
    
    private static readonly Vector2I originWindowSize = new(1280, 720);
    public bool isMobile;

    public override void _Ready()
    {
        ScreenInit();
        TayTool.Log("平台",$"当前平台: {OS.GetName()}");
        isMobile = OS.GetName() == "Android" || OS.GetName() == "iOS";
        just = gameState;
        TayTool.Log("状态",$"当前游戏状态: {gameState}");
        Instance = this;
        TayTool.Initialize();
        playerScene = [pointPlayerScene,linePlayerScene,polygonPlayerScene];
        hideMouse = DataManager.Instance.GetBoolConfig("hide_mouse_playing");
        glowDisabled = DataManager.Instance.GetBoolConfig("glow_disabled");
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
            TayTool.Log("状态",$"游戏状态切换: {gameState}");
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
        SignalManager.Instance.EmitSignal(SignalManager.SignalName.OnCardOut);
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
    
    private Dictionary<int, Vector2> _activeTouches = new();
    private int _lastPressedIndex = -1;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touch)
        {
            if (touch.Pressed)
            {
                _activeTouches[touch.Index] = touch.Position;
                _lastPressedIndex = touch.Index;   // ← 记录最近按下
            }
            else
            {
                _activeTouches.Remove(touch.Index);
                // 如果最后按下的那个抬起了，需要重新找一个
                if (_lastPressedIndex == touch.Index)
                    _lastPressedIndex = _activeTouches.Count > 0
                        ? _activeTouches.Keys.Max()   // 或换成"最近按下的另一个"
                        : -1;
            }
        }
        else if (@event is InputEventScreenDrag drag)
        {
            _activeTouches[drag.Index] = drag.Position;
        }
    }

    /// <summary> 获取最后开始触碰的点，没有则返回 null </summary>
    public Vector2? GetLastPressedPosition()
    {
        if (_lastPressedIndex == -1) return null;
        if (!_activeTouches.TryGetValue(_lastPressedIndex, out var pos)) return null;
        return pos;
    }
}