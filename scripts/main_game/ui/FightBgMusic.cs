using Godot;

public partial class FightBgMusic : AudioStreamPlayer
{
	[Export] AudioStream[] fightSounds;
	public GameManager gameManager;
	public SignalManager signalManager;
	public static FightBgMusic Instance{get; private set;}

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		gameManager = GetNode<GameManager>("/root/GameManager");
		signalManager =  GetNode<SignalManager>("/root/SignalManager");
		signalManager.OnCardOut += PlaySound;
	}

	public override void _ExitTree()
	{
		signalManager.OnCardOut -= PlaySound;
		Instance = null;
	}

	private void PlaySound()
	{
		if (IsPlaying()) return;
		Stream = fightSounds[GD.RandRange(0, fightSounds.Length - 1)];
		bool isRand = DataManager.Instance.GetBoolConfig("gaming_rand_play");
		if (Stream.ResourceName != "Rude Buster - Toby Fox.mp3" && isRand){
			Finished += () =>
			{
				Stream = fightSounds[GD.RandRange(0, fightSounds.Length - 1)];
				Play();
			};
		}
		else if (!isRand)
		{
			Finished += () =>
			{
				Play();
			};
		}
		Play();
	}

	public void SetTargetVolume(float volume)
	{
		volume = volume <= 0 ? 0.001f : volume;
		Tween volTw = GetTree().CreateTween();
		volTw.TweenProperty(this, "volume_db", Mathf.LinearToDb(volume), 0.3);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (gameManager.gameState == GameManager.GameState.GAMING && GetStreamPaused())
		{
			SetStreamPaused(false);
		}else if (gameManager.gameState != GameManager.GameState.GAMING && !GetStreamPaused())
		{
			SetStreamPaused(true);
		}
	}
}
