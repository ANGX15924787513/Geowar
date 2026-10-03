using Godot;

public partial class MenuBgMusicPlayer : AudioStreamPlayer
{
	[Export] public Godot.Collections.Array<AudioStream> bg_list = new();

	public override void _Process(double _delta)
	{
		VolumeLinear = GlobalAudioPlayer.Instance.menuBgVolume;
	}

	public void play_bg()
	{
		Stream = bg_list[GD.RandRange(0, bg_list.Count - 1)];
		Play();
	}
}
