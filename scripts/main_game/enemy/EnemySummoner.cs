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
	[Export] private float SummonWaitTime = 2f;
	private float SummonWaitTimeOffset = 0.2f;
	
	
	public override void _Ready()
	{
		SummonWaitTime = 2 - 1.8f * SingleGameData.Instance.currentDifficulty;
		ResetAllTimer();
	}
	public override void _Process(double delta)
	{
		if (GameManager.Instance.gameState == GameManager.GameState.GAMING)
		{
			UpdateAllTimer((float)delta);
			SummonWaitTime = 2 - 1.8f * SingleGameData.Instance.currentDifficulty;
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
			(float)GD.RandRange(GameManager.Instance.gameAreaX.X,GameManager.Instance.gameAreaX.Y),
			(float)GD.RandRange(GameManager.Instance.gameAreaY.X,GameManager.Instance.gameAreaY.Y));
		if (GetRand(superEnemyRate) && SingleGameData.Instance.currentDifficulty >=GameManager.Instance.SUPER_ENY)
		{
			enemy.SetScale(1.2f);
			enemy.MaxHealth *= GD.RandRange(2,5);
			enemy.Speed *= (float)GD.RandRange(1d,2d);
			enemy.Damage = (int)(enemy.Damage * GD.RandRange(2d, 5d));
			enemy.enemyType = Enemy.EnemyTypeEnum.SUPER;
		}
		else if(GetRand(susuperEnemyRate) && SingleGameData.Instance.currentDifficulty >= GameManager.Instance.SUSUPER_ENY)
		{
			enemy.SetScale(1.5f);
			enemy.MaxHealth *= GD.RandRange(5,10);
			enemy.Speed *= (float)GD.RandRange(3d,5d);
			enemy.Damage = (int)(enemy.Damage * GD.RandRange(5d, 8d));
			enemy.AddChild(superLight.Instantiate<PointLight2D>());
			enemy.enemyType = Enemy.EnemyTypeEnum.EXTREME;
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
			TayTool.Log("敌人",$"概率应在 0 到 100 之间, 当前值: {rate}",TayTool.LogTypeEnum.Err);
			return false;
		}
		int i = GD.RandRange(1, 100);
		return i <= rate;
	}
}
