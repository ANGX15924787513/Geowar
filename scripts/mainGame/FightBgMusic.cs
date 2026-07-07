using Godot;
using System;

public partial class FightBgMusic : AudioStreamPlayer
{
	public GameManager gameManager;
	public SignalManager signalManager;
	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		signalManager =  GetNode<SignalManager>("/root/SignalManager");
		signalManager.OnCardOut += PlaySound;
	}

	public override void _ExitTree()
	{
		signalManager.OnCardOut -= PlaySound;
	}

	private void PlaySound()
	{
		if (IsPlaying()) return;
		Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (gameManager.gameState == GameManager.GameState.GAMING && GetStreamPaused())
		{
			SetStreamPaused(false);
		}else if (gameManager.gameState != GameManager.GameState.GAMING && !GetStreamPaused())
		{
			SetStreamPaused(true);
		}
	}
}
