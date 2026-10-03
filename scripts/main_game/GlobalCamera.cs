using Godot;
using System.Collections.Generic;

public partial class GlobalCamera : Camera2D
{
	public static GlobalCamera Instance;
	private List<Node2D> targetNodes = new();

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		Enabled = false;
	}

	public override void _Process(double delta)
	{
		targetNodes.RemoveAll(t => t == null || !IsInstanceValid(t) || t.IsQueuedForDeletion());
		if (targetNodes.Count == 0) return;
		Position = targetNodes[0].GlobalPosition;
	}

	public override void _ExitTree()
	{
		Instance = null;
	}

	public void AddTarget(Node2D target)
	{
		targetNodes.Add(target);
	}
}
