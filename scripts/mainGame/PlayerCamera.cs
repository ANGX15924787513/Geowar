using Godot;

public partial class PlayerCamera : Camera2D
{
	public Vector2 toZoom;
	public override void _Process(double delta)
	{
		Zoom = Zoom.Lerp(toZoom, 0.05f);
	}
}
