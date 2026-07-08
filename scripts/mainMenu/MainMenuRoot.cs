using Godot;
using System;

public partial class MainMenuRoot : Node2D
{
	GameManager gameManager;
	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.gameState = GameManager.GameState.MAIN_MENU;
	}
}
