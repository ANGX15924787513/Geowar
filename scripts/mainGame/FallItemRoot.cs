using Godot;
using static TayTool;

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
	
	SignalManager signalManager;
	GameManager gameManager;
	
	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		gameManager = GetNode<GameManager>("/root/GameManager");
		signalManager.RequestCollectionSpawn += RandomlySpawn;
	}

	public override void _ExitTree()
	{
		signalManager.RequestCollectionSpawn -= RandomlySpawn;
	}

	public override void _Process(double delta)
	{
		if (gameManager.gameState != GameManager.GameState.GAMING) return;
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
		var pos = new Vector2(
				(float)GD.RandRange(gameManager.gameAreaX.X,gameManager.gameAreaX.Y),
				(float)GD.RandRange(gameManager.gameAreaY.X,gameManager.gameAreaY.Y));
		pck.GlobalPosition = pos;
		AddChild(pck);
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
			GD.PrintErr("掉落物设置错误:总概率超过100%");
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
