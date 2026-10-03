using Godot;

public partial class CursorArea : Area2D
{
	public override void _Process(double delta)
	{
		GlobalPosition = GetGlobalMousePosition();
	}
}
