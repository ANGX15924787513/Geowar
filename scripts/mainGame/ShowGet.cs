using Godot;
using System;

public partial class ShowGet : Node2D
{
	private SingleGameData gameData;
	
	private Label ironLabel;
	private Label goldLabel;
	private Label diamondLabel;

	private Label scorelabel;
	
	public override void _Ready()
	{
		gameData = GetNode<SingleGameData>("/root/mainGame/SingleGameData");
		ironLabel = GetNode<Label>("ironLabel");
		goldLabel = GetNode<Label>("goldLabel");
		diamondLabel = GetNode<Label>("diamondLabel");
		scorelabel = GetNode<Label>("../score");
		
		ironLabel.Text = gameData.ironCount.ToString();
		goldLabel.Text = gameData.goldCount.ToString();
		diamondLabel.Text = gameData.diamondCount.ToString();
		scorelabel.Text = $"得分:{gameData.ironCount + gameData.goldCount * 5 + gameData.diamondCount * 50}";

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
