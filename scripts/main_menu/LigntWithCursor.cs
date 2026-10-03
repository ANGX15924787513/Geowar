using Godot;

public partial class LigntWithCursor : RigidBody2D
{
	[Export(PropertyHint.Range, "0,20")] public float pull_strenth;

	public override void _Process(double _delta)
	{
		var to_pos = GetGlobalMousePosition() - GlobalPosition;
		LinearVelocity = to_pos * pull_strenth;
	}
}
