using Godot;

public partial class MainGame : Control
{
	SignalManager signalManager;
	AnimationPlayer animationPlayer;
	[Export] public PackedScene deathScreenScene;

	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		signalManager.OnPlayerDied += OnPlayerDied;
		signalManager.ShowDeathScreen += ShowDeathScreen;
		animationPlayer.AnimationFinished += OnAnimationFinished;
	}

	public override void _ExitTree()
	{
		signalManager.OnPlayerDied -= OnPlayerDied;
		signalManager.ShowDeathScreen -= ShowDeathScreen;
		animationPlayer.AnimationFinished -= OnAnimationFinished;
	}

	private void OnPlayerDied()
	{
		animationPlayer.Play("game_over");
	}

	private void OnAnimationFinished(StringName animName)
	{
		if (animName == "game_over")
			signalManager.EmitSignal(SignalManager.SignalName.ShowDeathScreen);
	}

	private void ShowDeathScreen()
	{
		var deathScreen = deathScreenScene.Instantiate();
		AddChild(deathScreen);
	}
}
