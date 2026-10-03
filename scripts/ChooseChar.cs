using Godot;

public partial class ChooseChar : Control
{
	public void destroy()
	{
		QueueFree();
	}

	public override void _Ready()
	{
		if (GameManager.Instance.charChose == true)
		{
			destroy();
			return;
		}
		GameManager.Instance.gameState = GameManager.GameState.CHOOSE_CARD;
	}

	public void emit_card_out()
	{
		GetNode<SignalManager>("/root/SignalManager").EmitSignal(SignalManager.SignalName.OnCardOut);
	}
}
