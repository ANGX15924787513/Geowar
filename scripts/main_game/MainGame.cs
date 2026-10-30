using Godot;

public partial class MainGame : Control
{
	AnimationPlayer animationPlayer;
	[Export] public PackedScene deathScreenScene;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		GetNode<WorldEnvironment>("WorldEnvironment").Environment.GlowEnabled = !GameManager.Instance.glowDisabled;
		SignalManager.Instance.OnPlayerDied += OnPlayerDied;
		SignalManager.Instance.ShowDeathScreen += ShowDeathScreen;
		animationPlayer.AnimationFinished += OnAnimationFinished;
	}

	public override void _ExitTree()
	{
		SignalManager.Instance.OnPlayerDied -= OnPlayerDied;
		SignalManager.Instance.ShowDeathScreen -= ShowDeathScreen;
		animationPlayer.AnimationFinished -= OnAnimationFinished;
	}

	private void OnPlayerDied()
	{
		animationPlayer.Play("game_over");
	}

	private void OnAnimationFinished(StringName animName)
	{
		if (animName == "game_over")
			SignalManager.Instance.EmitSignal(SignalManager.SignalName.ShowDeathScreen);
	}

	private void ShowDeathScreen()
	{
		var deathScreen = deathScreenScene.Instantiate();
		AddChild(deathScreen);
	}
}
