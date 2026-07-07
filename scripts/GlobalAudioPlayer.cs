using Godot;
using System;

public partial class GlobalAudioPlayer : Node
{
    public AudioStream buttonClickSound;

    public override void _Ready()
    {
        buttonClickSound = GD.Load("res://sounds/UIAudioPack/bong_001.ogg") as AudioStream;
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
