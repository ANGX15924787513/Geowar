using Godot;
using static FallItem;

public partial class SingleGameData : CanvasLayer
{
	public int ironCount;
	public int goldCount;
	public int diamondCount;
	
	[Export]private Label ironLabel;
	[Export]private Label goldLabel;
	[Export]private Label diamondLabel;
	
	[Export]private AnimationPlayer animationPlayer;
	
	public static SingleGameData Instance;
	
	public float currentDifficulty;

	private const int ToHighTime = 5 * 60; //seconds
	private float toHighTimer;
	public override void _Ready()
	{
		currentDifficulty = GameManager.Instance.difficulty;
		toHighTimer = ToHighTime;
		
		ironLabel.Text = ironCount.ToString();
		goldLabel.Text = goldCount.ToString();
		diamondLabel.Text = diamondCount.ToString();

		SignalManager.Instance.OnCollectedItem += UpdateInfo;
	}

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		SignalManager.Instance.OnCollectedItem -= UpdateInfo;
	}

	public override void _Process(double delta)
	{
		if (GameManager.Instance.gameState != GameManager.GameState.GAMING)
		{
			Visible = false;
		}
		else
		{
			Visible = true;
			if (!(toHighTimer > 0)) return;
			toHighTimer -= (float)delta;
			currentDifficulty = GameManager.Instance.difficulty + (1 - GameManager.Instance.difficulty) * ((ToHighTime - toHighTimer) / ToHighTime);
		}
	}

	private void UpdateInfo(FallItemType fallItemType)
	{
		switch (fallItemType)
		{
			case FallItemType.IRON:
				ironCount++;
				if (animationPlayer.IsPlaying()) break;
				animationPlayer.Play("iron_jump");
				break;
			case FallItemType.GOLD:
				goldCount++;
				if (animationPlayer.IsPlaying()) break;
				animationPlayer.Play("gold_jump");
				break;
			case FallItemType.DIAMOND:
				diamondCount++;
				if (animationPlayer.IsPlaying()) break;
				animationPlayer.Play("diamond_jump");
				break;
		}
		ironLabel.Text = ironCount.ToString();
		goldLabel.Text = goldCount.ToString();
		diamondLabel.Text = diamondCount.ToString();
	}
}
