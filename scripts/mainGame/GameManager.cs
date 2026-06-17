using Godot;
using System;

public partial class GameManager : Node
{
    public bool charChose { get; set; } = false;
    
    public enum PlayerType
    {
        POINT,
        LINE,
        POLYGON
    }
    public PlayerType playerType;

    public void SummonPlayer(PackedScene playerScene, SceneTree tree)
    {
        var player = (Node2D)playerScene.Instantiate();
        player.Position += new Vector2(-50, -50);
        tree.CurrentScene.AddChild(player);
    }
}