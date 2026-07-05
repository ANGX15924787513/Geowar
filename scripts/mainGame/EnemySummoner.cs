using Godot;
using System;

public partial class EnemySummoner : Node2D
{
	[Export] private PackedScene pointEnemy;
	
	//Timer
	private float _pointSummonTimer;
	[Export] private float pointSummonWaitTime = 2f;
	private float pointSummonWaitTimeOffset = 1f;
	
	
	GameManager gameManager;
	
	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		ResetAllTimer();
	}
	public override void _Process(double delta)
	{
		if (gameManager.gameState == GameManager.GameState.GAMING)
		{
			UpdateAllTimer((float)delta);
		}
	}
	private void ResetAllTimer()
	{
		ResetPointSummonTimer();
	}
	
	private void UpdateAllTimer(float delta)
	{
		_pointSummonTimer -= delta;
		//else update...

		if (_pointSummonTimer <= 0f)
		{
			SummonEnemy(pointEnemy);
			ResetPointSummonTimer();
		}
		//else dealing and reset...
	}

	private void SummonEnemy(PackedScene packedScene)
	{
		var enemy = packedScene.Instantiate() as Node2D;
		enemy.GlobalPosition = new Vector2(
			(float)GD.RandRange(-1700f,2600f),
			(float)GD.RandRange(-1050f,1136f));
		AddChild(enemy);
	}

	private void ResetPointSummonTimer()
    	{
    		_pointSummonTimer = (float)GD.RandRange(
    			pointSummonWaitTime - pointSummonWaitTimeOffset,
    			pointSummonWaitTime + pointSummonWaitTimeOffset);
    	}
}
