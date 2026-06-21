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
    public GameState gameState;
    public bool charChose { get; set; } = false;
    public float playerSpeed = 500f;
    public float playerRotateSpeed = 1f;
    
    public enum PlayerType
    {
        POINT,
        LINE,
        POLYGON
    }
    public PlayerType playerType;
    public int playerTypeCount = 3;
    
    public List<PackedScene> playerScene;

    public override void _Ready()
    {
        playerScene =
        [
            GD.Load<PackedScene>("res://scenes/main/point/pointPlayer.tscn"), //Point
            GD.Load<PackedScene>("res://scenes/main/line/linePlayer.tscn"), //Line
            GD.Load<PackedScene>("res://scenes/main/polygon/polygonPlayer.tscn") //Polygon
        ];
    }

    public void SummonPlayer(SceneTree tree)
    {
        if (playerScene.Count == 0)
        {
            _Ready();
        }
        var player = (Node2D)playerScene[(int)playerType].Instantiate();
        Camera2D camera = new Camera2D();
        camera.PositionSmoothingEnabled = true;
        player.AddChild(camera);
        tree.CurrentScene.AddChild(player);
    }
}