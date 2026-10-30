using Godot;

public partial class MobileJoystick : CanvasLayer
{
	private bool isMobile;
	[Export] private TouchScreenButton tsButton;
	public override void _Ready()
	{
		isMobile = OS.GetName() == "Android" || OS.GetName() == "iOS";
		TayTool.Log("移动摇杆",$"移动端: {isMobile}");
		UpdateVisible(GameManager.Instance.gameState);
		GameManager.Instance.OnGameStateChange += UpdateVisible;
	}

	public override void _ExitTree()
	{
		GameManager.Instance.OnGameStateChange -= UpdateVisible;
	}

	private void UpdateVisible(GameManager.GameState newState)
	{
		if (isMobile)
		{
			Visible = newState == GameManager.GameState.GAMING;
			if(newState == GameManager.GameState.GAMING) tsButton.GetParent<CanvasItem>().Visible = Player.Instance.currentPlayerType == Player.PlayerTypeEnum.LINE;
		}
		else
		{
			Visible = false;
			ProcessMode = ProcessModeEnum.Disabled;
		}
	}
}
