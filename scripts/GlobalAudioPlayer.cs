using Godot;
using System;

public partial class GlobalAudioPlayer : Node
{
    public AudioStream buttonClickSound;
    public AudioStream collectXPSound;

    public override void _Ready()
    {
        buttonClickSound = GD.Load<AudioStream>("res://sounds/UIAudioPack/bong_001.ogg");
        collectXPSound = GD.Load<AudioStream>("res://sounds/MCSound/orb.ogg");
    }
    public async void PlayAudio(AudioStream stream,float volume = 1)
    {
        if (stream == null)
        {
            GD.PrintErr("Audio stream is null");
            return;
        }
        AudioStreamPlayer player = new AudioStreamPlayer();
        player.Stream = stream;
        player.VolumeLinear = volume;
        AddChild(player);
        player.Play();
        await ToSignal(player, "finished");
        RemoveChild(player);
    }
}
