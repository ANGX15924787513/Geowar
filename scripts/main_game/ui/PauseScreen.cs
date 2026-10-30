using Godot;

public partial class PauseScreen : CanvasLayer
{
	private AnimationPlayer animationPlayer;
	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		GetTree().Root.FocusExited += FEPauseHandler;
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause") && !animationPlayer.IsPlaying())
		{
			PauseHandler();
		}
	}

	private void FEPauseHandler()
	{
		if (GameManager.Instance.gameState == GameManager.GameState.GAMING)
		{
			PauseHandler();
		}
	}

	private async void PauseHandler()
	{
		if (GameManager.Instance.gameState == GameManager.GameState.GAMING)
		{
			GameManager.Instance.gameState = GameManager.GameState.GAME_PAUSED;
			animationPlayer.Play("enter");
			await ToSignal(animationPlayer, "animation_finished");
		}
		else
		{
			OnContinueButtonPressed();
		}
	}
	
	private async void OnContinueButtonPressed()
	{
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		if (GameManager.Instance.gameState == GameManager.GameState.GAME_PAUSED && !animationPlayer.IsPlaying())
		{
			animationPlayer.Play("exit");
			await ToSignal(animationPlayer, "animation_finished");
			GameManager.Instance.gameState = GameManager.GameState.GAMING;
		}
	}

	private void OnMenuButtonPressed()
	{
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		if (GameManager.Instance.gameState == GameManager.GameState.GAME_PAUSED)
		{
			GetNode("/root/EasyTransition").Call("transition_to", "res://scenes/main_menu.tscn", 1);
			DataManager.Instance.Add(FallItem.FallItemType.GOLD,    SingleGameData.Instance.goldCount + SingleGameData.Instance.ironCount / 4);
			DataManager.Instance.Add(FallItem.FallItemType.DIAMOND, SingleGameData.Instance.diamondCount);
			GameManager.Instance.gameState = GameManager.GameState.MAIN_MENU;
		}
	}
}
