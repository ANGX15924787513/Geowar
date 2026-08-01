using Godot;

public partial class PauseScreen : CanvasLayer
{
	private GameManager gameManager;
	private DataManager dataManager;
	private SingleGameData gameData;
	private AnimationPlayer animationPlayer;
	private GlobalAudioPlayer globalAudioPlayer;
	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		globalAudioPlayer = GetNode<GlobalAudioPlayer>("/root/GlobalAudioPlayer");
		dataManager = GetNode<DataManager>("/root/DataManager");
		gameData = GetNode<SingleGameData>("/root/mainGame/SingleGameData");
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause") && !animationPlayer.IsPlaying())
		{
			PauseHandler();
		}
	}

	private void PauseHandler()
	{
		if (gameManager.gameState == GameManager.GameState.GAMING)
		{
			gameManager.gameState = GameManager.GameState.GAME_PAUSED;
			animationPlayer.Play("enter");
		}
		else
		{
			OnContinueButtonPressed();
		}
	}
	
	private async void OnContinueButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		if (gameManager.gameState == GameManager.GameState.GAME_PAUSED)
		{
			animationPlayer.Play("exit");
			await ToSignal(animationPlayer, "animation_finished");
			gameManager.gameState = GameManager.GameState.GAMING;
		}
	}

	private void OnMenuButtonPressed()
	{
		globalAudioPlayer.PlayAudio(globalAudioPlayer.buttonClickSound);
		if (gameManager.gameState == GameManager.GameState.GAME_PAUSED)
		{
			GetNode("/root/EasyTransition").Call("transition_to", "res://scenes/main_menu.tscn", 1);
			dataManager.Add(FallItem.FallItemType.GOLD,    gameData.goldCount + gameData.ironCount / 4);
			dataManager.Add(FallItem.FallItemType.DIAMOND, gameData.diamondCount);
			gameManager.gameState = GameManager.GameState.MAIN_MENU;
		}
	}
}
