using Godot;

public partial class MainMenuRoot : Control
{
	public static MainMenuRoot Instance;
	[Export]private ColorRect _colorRect;
	public override void _Ready()
	{
		GameManager.Instance.gameState = GameManager.GameState.MAIN_MENU;
		GetNode<WorldEnvironment>("WorldEnvironment").Environment.GlowEnabled = !GameManager.Instance.glowDisabled;
	}
	public override void _EnterTree() => Instance = this;
	public override void _ExitTree() => Instance = null;

	public void SwitchCentreVisible(bool visible)
	{
		MenuButtons.Instance.Visible = visible;
		PhysicShow.Instance.parent.Visible = visible;
		Title.Instance.Visible = visible;
		Tween bgColorTween = GetTree().CreateTween();
		bgColorTween.TweenProperty(
			_colorRect,
			"color",
			visible ? Colors.Black : Colors.Black.Lightened(0.3f),
			0.3);
	}
}
