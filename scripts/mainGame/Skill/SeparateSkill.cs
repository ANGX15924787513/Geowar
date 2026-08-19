using Godot;
using System;

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
		}
	}

	private void ThrowTnt()
	{
		foreach (Node node in GetNode("/root/mainGame/enemyRoot").GetChildren())
		{
			if (node is not Enemy enemy) continue;
			if (iconRect.Duplicate() is not TextureRect icon)
			{
				GD.PrintErr("TNT图片不是TextureRect");
				return;
			}
			icon.SelfModulate = new Color(Colors.White,0);
			GetTree().CurrentScene.AddChild(icon);
			enemy.Speed = 0;
			Tween tw = GetTree().CreateTween();
			tw.TweenProperty(icon, "global_position", enemy.GlobalPosition, 0.6);
			Tween tw2 = GetTree().CreateTween();
			tw2.TweenProperty(icon, "self_modulate", Colors.White, 0.6);
			tw.SetEase(Tween.EaseType.InOut);
			tw.Finished += () =>
			{
				Tween tw1 = GetTree().CreateTween();
				tw1.TweenProperty(icon, "scale", icon.Scale * 1.5f, 0.4);
				tw1.SetEase(Tween.EaseType.InOut);
				tw1.Finished += () => {
					enemy.GotHurt(99999999, null);
					TayTool.CollideLight(icon.Position,Colors.Red, GetTree());
					icon.QueueFree();
				};
			};
		}
	}
}
