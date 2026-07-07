using Godot;
using System;

public partial class FallItemRoot : Node2D
{
	[Export] private PackedScene ironScene;
	[Export] private PackedScene goldScene;
	[Export] private PackedScene diamondScene;
	[Export] private PackedScene emeraldScene;
	
	[Export] private float ironSpawnProb = 100;
	[Export] private float goldSpawnProb = 20;
	[Export] private  float diamondSpawnProb = 2;
	
	SignalManager signalManager;
	
	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		signalManager.RequestCollectionSpawn += RandomlySpawn;
	}

	public override void _ExitTree()
	{
		signalManager.RequestCollectionSpawn -= RandomlySpawn;
	}

	private void RandomlySpawn(Vector2 pos)
	{
		int rand = GD.RandRange(0, 100);
		if (rand <= ironSpawnProb)
		{
			Node2D scene = ironScene.Instantiate() as Node2D;
			scene.GlobalPosition = pos;
			AddChild(scene);
		}
		if (rand <= goldSpawnProb)
		{
			Node2D scene = goldScene.Instantiate() as Node2D;
			scene.GlobalPosition = pos;
			AddChild(scene);
		}

		if (rand <= diamondSpawnProb)
		{
			Node2D scene = diamondScene.Instantiate() as Node2D;
			scene.GlobalPosition = pos;
			AddChild(scene);
		}
	}
}
