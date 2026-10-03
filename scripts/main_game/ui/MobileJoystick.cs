using Godot;

public partial class MobileJoystick : CanvasLayer
{
	public override void _Ready()
	{
		if (OS.GetName() == "Android" || OS.GetName() == "iOS")
		{
			Visible = true;
		}
		else
		{
			Visible = false;
			ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	public override void _Process(double _delta)
	{
		Visible = GameManager.Instance.gameState == GameManager.GameState.GAMING;
	}
}
