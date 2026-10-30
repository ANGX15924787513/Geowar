using Godot;

public partial class FallItemRoot : Node2D
{
	[Export] private PackedScene ironScene;
	[Export] private PackedScene goldScene;
	[Export] private PackedScene diamondScene;
	[Export] private PackedScene emeraldScene;
	[Export] private PackedScene healPackageScene;
	
	[Export] private float ironSpawnProb = 78;
	[Export] private float goldSpawnProb = 20;
	[Export] private  float diamondSpawnProb = 2;
	[Export] private float healPckSumGap = 7f;
	private float _healTimer;

	public static FallItemRoot Instance;
	
	public override void _Ready()
	{
		SignalManager.Instance.RequestCollectionSpawn += RandomlySpawn;
	}

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		SignalManager.Instance.RequestCollectionSpawn -= RandomlySpawn;
		Instance = null;
	}

	public override void _Process(double delta)
	{
		if (GameManager.Instance.gameState != GameManager.GameState.GAMING) return;
		if (_healTimer > 0f)
		{
			_healTimer -= (float)delta;
		}
		else
		{
			_healTimer = healPckSumGap;
			SpawnHealPck();
		}
	}

	private void SpawnHealPck()
	{
		var pck = healPackageScene.Instantiate<Node2D>();
		AddChild(pck);
		var pos = new Vector2(
				(float)GD.RandRange(GameManager.Instance.gameAreaX.X,GameManager.Instance.gameAreaX.Y),
				(float)GD.RandRange(GameManager.Instance.gameAreaY.X,GameManager.Instance.gameAreaY.Y));
		pck.GlobalPosition = pos;
	}

	private void RandomlySpawn(Vector2 pos,Enemy.EnemyTypeEnum enemyType)
	{
		int start;
		switch (enemyType)
		{
			case Enemy.EnemyTypeEnum.NORMAL:
				start = 0;
				break;
			case Enemy.EnemyTypeEnum.SUPER:
				start = 20;
				break;
			case Enemy.EnemyTypeEnum.EXTREME:
				start = 50;
				break;
			default:	
				start = 0;	
				break;
		}
		int rand = GD.RandRange(start, 100);
		if (ironSpawnProb + goldSpawnProb + diamondSpawnProb > 100)
		{
			TayTool.Log("掉落物","掉落物设置错误: 总概率超过 100%",TayTool.LogTypeEnum.Err);
			return;
		}
		if (rand <= ironSpawnProb)
		{
			Node2D scene = ironScene.Instantiate() as Node2D;
				scene.GlobalPosition = pos;
				AddChild(scene);
		}
		else if (rand <= ironSpawnProb + goldSpawnProb)
		{
			Node2D scene = goldScene.Instantiate() as Node2D;
			scene.GlobalPosition = pos;
			AddChild(scene);
		}
		else if (rand <= ironSpawnProb + goldSpawnProb + diamondSpawnProb)
		{
			Node2D scene = diamondScene.Instantiate() as Node2D;
			scene.GlobalPosition = pos;
			AddChild(scene);
		}
	}
}
