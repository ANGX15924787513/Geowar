using Godot;
using System;
using static FallItem;

public partial class SingleGameData : CanvasLayer
{
	public int ironCount;
	public int goldCount;
	public int diamondCount;
	
	private Label ironLabel;
	private Label goldLabel;
	private Label diamondLabel;
	
	private AnimationPlayer animationPlayer;
	
	private SignalManager signalManager;
	private GameManager gameManager;
	private DataManager dataManager;
	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		gameManager = GetNode<GameManager>("/root/GameManager");
		dataManager = GetNode<DataManager>("/root/DataManager");
		ironLabel = GetNode<Label>("ironLabel");
		goldLabel = GetNode<Label>("goldLabel");
		diamondLabel = GetNode<Label>("diamondLabel");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		ironLabel.Text = ironCount.ToString();
		goldLabel.Text = goldCount.ToString();
		diamondLabel.Text = diamondCount.ToString();

		signalManager.OnCollectedItem += UpdateInfo;
		signalManager.OnPlayerDied += SaveToDataManager;
	}

	public override void _ExitTree()
	{
		signalManager.OnCollectedItem -= UpdateInfo;
		signalManager.OnPlayerDied -= SaveToDataManager;
	}

	public override void _Process(double delta)
	{
		if (gameManager.gameState != GameManager.GameState.GAMING)
		{
			Visible = false;
		}
		else
		{
			Visible = true;
		}
	}

	private void SaveToDataManager()
	{
		dataManager.Add(FallItemType.IRON,    ironCount);
		dataManager.Add(FallItemType.GOLD,    goldCount);
		dataManager.Add(FallItemType.DIAMOND, diamondCount);
	}

	private void UpdateInfo(FallItemType fallItemType)
	{
		switch (fallItemType)
		{
			case FallItemType.IRON:
				animationPlayer.Play("iron_jump");
				ironCount++;
				break;
			case FallItemType.GOLD:
				animationPlayer.Play("gold_jump");
				goldCount++;
				break;
			case FallItemType.DIAMOND:
				animationPlayer.Play("diamond_jump");
				diamondCount++;
				break;
		}
		ironLabel.Text = ironCount.ToString();
		goldLabel.Text = goldCount.ToString();
		diamondLabel.Text = diamondCount.ToString();
	}
}
