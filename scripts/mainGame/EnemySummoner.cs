using Godot;
using Godot.Collections;

public partial class EnemySummoner : Node2D
{
	[Export] private Array<PackedScene> Enemies;
	[Export(PropertyHint.None,"suffix:%")] private int superEnemyRate = 10;
	[Export(PropertyHint.None,"suffix:%")] private int susuperEnemyRate = 1;

	[Export] private PackedScene superLight;
	
	//Timer
	private float _SummonTimer;
	[Export] private float SummonWaitTime = 1.3f;
	private float SummonWaitTimeOffset = 0.3f;
	
	
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
		_SummonTimer -= delta;
		//else update...

		if (_SummonTimer <= 0f)
		{
			SummonEnemy(Enemies.PickRandom());
			ResetPointSummonTimer();
		}
		//else dealing and reset...
	}

	private void SummonEnemy(PackedScene packedScene)
	{
		var enemy = packedScene.Instantiate() as Enemy;
		enemy.GlobalPosition = new Vector2(
			(float)GD.RandRange(gameManager.gameAreaX.X,gameManager.gameAreaX.Y),
			(float)GD.RandRange(gameManager.gameAreaY.X,gameManager.gameAreaY.Y));
		if (GetRand(superEnemyRate))
		{
			enemy.MaxHealth *= GD.RandRange(2,5);
			enemy.Speed *= (float)GD.RandRange(1d,2d);
			enemy.Damage = (int)(enemy.Damage * GD.RandRange(2d, 5d));
		}
		else if(GetRand(susuperEnemyRate))
		{
			enemy.MaxHealth *= GD.RandRange(5,10);
			enemy.Speed *= (float)GD.RandRange(3d,5d);
			enemy.Damage = (int)(enemy.Damage * GD.RandRange(5d, 8d));
			enemy.AddChild(superLight.Instantiate<PointLight2D>());
		}
		AddChild(enemy);
	}

	private void ResetPointSummonTimer()
    	{
    		_SummonTimer = (float)GD.RandRange(
    			SummonWaitTime - SummonWaitTimeOffset,
    			SummonWaitTime + SummonWaitTimeOffset);
    	}

	private bool GetRand(int rate)
	{
		if (rate <= 0 || rate > 100)
		{
			GD.PrintErr("概率应在 0 到 100 之间");
			return false;
		}
		int i = GD.RandRange(1, 100);
		return i <= rate;
	}
}
