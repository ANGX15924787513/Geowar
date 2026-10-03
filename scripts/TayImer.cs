using Godot;

public partial class TayImer : Timer
{
	public override void _Ready()
	{
		GameManager.Instance.OnGameStateChange += on_game_state_change;
	}

	public override void _ExitTree()
	{
		GameManager.Instance.OnGameStateChange -= on_game_state_change;
	}

	public void on_game_state_change(GameManager.GameState new_state)
	{
		if (new_state == GameManager.GameState.GAMING) //GAMING
		{
			SetProcessMode(ProcessModeEnum.Inherit);
		}
		else
		{
			SetProcessMode(ProcessModeEnum.Disabled);
		}
	}
}
