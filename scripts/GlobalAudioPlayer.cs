using Godot;

public partial class GlobalAudioPlayer : Node
{
    public AudioStream buttonClickSound;
    public AudioStream collectXPSound;
    public AudioStream playerHealingSound;
    public AudioStream playerHurtSound;

    public float PlayerAttackVolume;
    private DataManager dataManager;

    public override void _Ready()
    {
        buttonClickSound = GD.Load<AudioStream>("res://sounds/UIAudioPack/bong_001.ogg");
        collectXPSound = GD.Load<AudioStream>("res://sounds/MCSound/orb.ogg");
        playerHurtSound = GD.Load<AudioStream>("res://sounds/MCSound/fire_hurt1.ogg");
        playerHealingSound = GD.Load<AudioStream>("res://sounds/MCSound/drink.ogg");
        dataManager = GetNode<DataManager>("/root/DataManager");
        
        PlayerAttackVolume = dataManager.IsExists("player_attack_volume")
            ? dataManager.GetFloatConfig("player_attack_volume")
            : 0.6f;
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
