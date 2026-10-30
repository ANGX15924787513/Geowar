using System;
using Godot;

public partial class SeparateSkill : Control
{
	private string name,desc,id,iconPath;
	private float coolDown;
	
	private Label nameLabel;
	private Label descLabel;
	private ProgressBar coolDownProgressBar;
	private Timer coolDownTimer;
	private TextureRect iconRect;
	public override void _Ready()
	{
		Visible = GameManager.Instance.gameState == GameManager.GameState.GAMING;
		
		nameLabel = GetNode<Label>("VB/NameLabel");
		descLabel = GetNode<Label>("VB/DescLabel");
		coolDownProgressBar = GetNode<ProgressBar>("ProgressBar");
		coolDownTimer = GetNode<Timer>("Timer");
		iconRect = GetNode<TextureRect>("TextureRect");
		
		nameLabel.Text = name;
		descLabel.Text = desc;
		coolDownTimer.WaitTime = coolDown;
		coolDownProgressBar.Value = coolDown;
		iconRect.Texture = GD.Load<CompressedTexture2D>(iconPath);
		coolDownTimer.Start();
		coolDownTimer.Timeout += RunSkill;
		GameManager.Instance.OnGameStateChange += GameStateChangeHandler;
	}

	public override void _ExitTree()
	{
		GameManager.Instance.OnGameStateChange -= GameStateChangeHandler;
	}

	public void Setup(Skill skill)
	{
		name = skill.name;
		desc = skill.desc;
		coolDown = skill.cooldown;
		id = skill.id;
		iconPath = skill.iconPath;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		coolDownProgressBar.Value = (coolDown - coolDownTimer.GetTimeLeft())/coolDown * 100;
	}

	private void GameStateChangeHandler(GameManager.GameState newState)
	{
		Visible = newState == GameManager.GameState.GAMING;
	}

	private void RunSkill()
	{
		switch (id)
		{
			case "skill_tnt":
				ThrowTnt();
				break;
			case "skill_collect_all_coins":
				CollectCoins();
				break;
		}
	}
	
	[ExportGroup("Skill Scenes & Settings")]
	[ExportSubgroup("TNT")]
	[Export] private PackedScene tntScene;
	[Export] private AudioStream tntStartSound;
	private void ThrowTnt()
	{
		//同步相机视角
		GlobalCamera.Instance.Zoom = PlayerCamera.Instance.Zoom;
		GlobalCamera.Instance.Position = PlayerCamera.Instance.GlobalPosition;
		GlobalCamera.Instance.PositionSmoothingEnabled = true;
		GlobalCamera.Instance.Enabled = true;
		GlobalCamera.Instance.MakeCurrent();
		
		//使玩家无敌
		Player.Instance.isInvincible = true;
		GlobalAudioPlayer.Instance.PlayAudio(tntStartSound,1,true);

		const float throwDelta = 0.2f;
		int c = 0;
		foreach (Node node in GetNode("/root/mainGame/enemyRoot").GetChildren())
		{
			if (node is not Enemy enemy) continue;
			var tnt = tntScene.Instantiate<TNTAim>();
			GetTree().CurrentScene.AddChild(tnt);
			tnt.Visible = false;
			var throwTimer = GetTree().CreateTimer(c * throwDelta);
			throwTimer.Timeout += ThrowTimerOnTimeout;
			GlobalCamera.Instance.AddTarget(enemy);

			void ThrowTimerOnTimeout()
			{
				tnt.Visible = true;
				tnt.GlobalPosition = GameManager.Instance.gameAreaCentre;
				tnt.aimNode = enemy;
				tnt.GlobalPosition = GetGlobalMousePosition();
			
				//5s后强制炸
				var timer = GetTree().CreateTimer(5);
				timer.Timeout += tnt.Explode;
			}

			c++;
		}
	}
	
	private void CollectCoins()
	{
		var children = FallItemRoot.Instance.GetChildren();
		if(children.Count == 0) return;
		var tw = GetTree().CreateTween();
		tw.SetParallel();
		foreach (var node in children)
		{
			if (node is not Node2D coin) return;
			tw.TweenProperty(coin,"global_position",Player.Instance.GlobalPosition + TayTool.GetRandVec2I(300),0.3f);
		}
	}
}
