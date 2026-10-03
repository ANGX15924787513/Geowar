using Godot;

public partial class StaticBody2d : CharacterBody2D
{
	public override void _Process(double delta)
	{
		Rotate(Mathf.Pi * (float)delta);
	}
}
