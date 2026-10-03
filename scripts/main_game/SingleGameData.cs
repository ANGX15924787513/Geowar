using Godot;
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
	
	public static SingleGameData Instance;
	
	public float currentDifficulty;

	private const int ToHighTime = 5 * 60; //seconds
	private float toHighTimer;
	public override void _Ready()
	{
		signalManager = GetNode<SignalManager>("/root/SignalManager");
		gameManager = GetNode<GameManager>("/root/GameManager");
		ironLabel = GetNode<Label>("ironLabel");
		goldLabel = GetNode<Label>("goldLabel");
		diamondLabel = GetNode<Label>("diamondLabel");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		currentDifficulty = gameManager.difficulty;
		toHighTimer = ToHighTime;
		
		ironLabel.Text = ironCount.ToString();
		goldLabel.Text = goldCount.ToString();
		diamondLabel.Text = diamondCount.ToString();

		signalManager.OnCollectedItem += UpdateInfo;
	}

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		signalManager.OnCollectedItem -= UpdateInfo;
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
			if (!(toHighTimer > 0)) return;
			toHighTimer -= (float)delta;
			currentDifficulty = gameManager.difficulty + (1 - gameManager.difficulty) * ((ToHighTime - toHighTimer) / ToHighTime);
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
