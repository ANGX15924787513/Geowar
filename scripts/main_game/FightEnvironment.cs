using Godot;
using Environment = Godot.Environment;

public partial class FightEnvironment : WorldEnvironment
{
	private Environment environment;
	[Export] private float pauseGlowStrength;
	[Export] private float deathGlowStrength;
	[Export] private float gamingGlowStrength;
	public override void _Ready()
	{
		environment = Environment;
	}
	
	public override void _Process(double delta)
	{
		environment.GlowIntensity = GameManager.Instance.gameState switch
		{
			GameManager.GameState.GAME_PAUSED => float.Lerp(environment.GlowIntensity, pauseGlowStrength, 0.05f),
			GameManager.GameState.GAMING => float.Lerp(environment.GlowIntensity, gamingGlowStrength, 0.05f),
			GameManager.GameState.GAME_OVER => float.Lerp(environment.GlowIntensity, deathGlowStrength, 0.05f),
			_ => environment.GlowIntensity
		};
	}
}
