using Godot;

public partial class LineHpBar : Line2D
{
	private AnimationPlayer ani;
	private SignalManager signalManager;

	public override void _Ready()
	{
		ani = GetNode<AnimationPlayer>("AnimationPlayer");
		ani.Play("shake_heal");
		DefaultColor = Colors.Green;
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		signalManager.OnPlayerHealthChanged += refresh;
	}

	public override void _ExitTree()
	{
		signalManager.OnPlayerHealthChanged -= refresh;
	}

	private void refresh(int change, int hp, int max_hp)
	{
		if (change != 0)
		{
			ani.Play(change < 0 ? "shake_hurt" : "shake_heal");
			var new_color = Colors.Red.Lerp(Colors.Green, (float)hp / max_hp);
			var tw = GetTree().CreateTween();
			tw.TweenProperty(this, "default_color", new_color, 0.3f);
		}
	}
}
