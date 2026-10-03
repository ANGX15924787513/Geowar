using Godot;

public partial class DeathScrButtontoMainMenu : Button
{
	public void switch_to_main_menu()
	{
		GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
		GetNode("/root/EasyTransition").Call("transition_to", "res://scenes/main_menu.tscn");
	}
}
