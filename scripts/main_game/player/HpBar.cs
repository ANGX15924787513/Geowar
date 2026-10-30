using Godot;

public partial class HpBar : CanvasLayer
{
	private Label health_label;

	public override void _Ready()
	{
		health_label = GetNode<Label>("Node2D/Label2");
		SignalManager.Instance.OnPlayerHealthChanged += on_player_health_changed;
	}

	public override void _ExitTree()
	{
		SignalManager.Instance.OnPlayerHealthChanged -= on_player_health_changed;
	}

	public override void _Process(double _delta)
	{
		if (GameManager.Instance.gameState != GameManager.GameState.GAMING)
		{
			Visible = false;
		}
		else
		{
			Visible = true;
		}
	}

	private void on_player_health_changed(int _damage, int health, int max_health)
	{
		health_label.Text = health + "/" + max_health;
	}
}
