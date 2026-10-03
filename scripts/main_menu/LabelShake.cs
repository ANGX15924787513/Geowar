using Godot;

public partial class LabelShake : Label
{
	public async void ShowNotEnough()
	{
		string cathe = Text;
		Text = "穷";
		await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
		Text = cathe;
	}
}
