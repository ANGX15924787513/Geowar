using Godot;

public partial class Entrance : Control
{
	[Export] public PackedScene main_menu;

	public override void _Ready()
	{
		GameManager.Instance.gameState = GameManager.GameState.STARTING;
		GetNode<WorldEnvironment>("WorldEnvironment").Environment.GlowEnabled = !GameManager.Instance.glowDisabled;
	}

	public void change_to_menu()
	{
		GetNode("/root/EasyTransition").Call("transition_to", main_menu.ResourcePath, 2f, 7);
	}
}
