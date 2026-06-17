using Godot;
using System;

public partial class PolygonSync : CollisionPolygon2D
{
	private Polygon2D polygonShape;
	public override void _Ready()
	{
		polygonShape = GetNode<Polygon2D>("Polygon2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		polygonShape.Polygon = Polygon;
	}
}
