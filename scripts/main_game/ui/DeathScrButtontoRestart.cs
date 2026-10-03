using Godot;

public partial class DeathScrButtontoRestart : Button
{
	public void restart()
	{
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound, 1.0f, false);
		GetNode("/root/EasyTransition").Call("transition_to", "res://scenes/main_game.tscn");
	}
}
