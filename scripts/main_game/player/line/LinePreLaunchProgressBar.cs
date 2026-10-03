using Godot;

public partial class LinePreLaunchProgressBar : ProgressBar
{
	private float to;

	public override void _Process(double _delta)
	{
		if (Input.IsActionPressed("pre_launch_bullet"))
		{
			to = 100;
		}
		else
		{
			to = 0;
		}
		Value = Mathf.Lerp((float)Value, to, 0.08f);
	}
}
