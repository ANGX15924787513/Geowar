using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class GlobalAudioPlayer : Node
{
    public AudioStream buttonClickSound;
    public AudioStream collectXPSound;
    public AudioStream playerHealingSound;
    public AudioStream onFireSound;
    public AudioStream boomSound;
    public Array<AudioStream> playerHurtSounds = new();

    public float PlayerAttackVolume;
    public float menuBgVolume;
    public float gamingBgVolume;
    
    private System.Collections.Generic.Dictionary<AudioStream,int> soundCounters = new();
    
    HashSet<AudioStream> playingStreams = new();
    System.Collections.Generic.Dictionary<AudioStream, AudioStreamPlayer> players = new();
    System.Collections.Generic.Dictionary<AudioStreamPlayer,float> fcSavedVolumes = new();
    
    private DataManager dataManager;
    public static GlobalAudioPlayer Instance{get; private set;}

    public override void _Ready()
    {
        Instance = this;
        buttonClickSound = GD.Load<AudioStream>("res://sounds/UIAudioPack/bong_001.ogg");
        collectXPSound = GD.Load<AudioStream>("res://sounds/MCSound/orb.ogg");
        onFireSound = GD.Load<AudioStream>("res://sounds/MCSound/fire_hurt1.ogg");
        playerHealingSound = GD.Load<AudioStream>("res://sounds/MCSound/drink.ogg");
        boomSound = GD.Load<AudioStream>("res://sounds/MCSound/explode1.ogg");
        for (int i =1; i < 5; i++) { playerHurtSounds.Add(GD.Load<AudioStream>($"res://sounds/MCSound/villager/hit{i}.ogg")); }
        dataManager = GetNode<DataManager>("/root/DataManager");
        
        VolumeInit();
    }

    private void VolumeInit()
    {
        PlayerAttackVolume = dataManager.IsExistsSetting("player_attack_volume")
            ? dataManager.GetFloatConfig("player_attack_volume")
            : 0.6f;

        menuBgVolume = dataManager.IsExistsSetting("mainmenu_bg_volume")
            ? dataManager.GetFloatConfig("mainmenu_bg_volume")
            : 1f;
        
        gamingBgVolume = dataManager.IsExistsSetting("gaming_bg_volume")
            ? dataManager.GetFloatConfig("gaming_bg_volume")
            : 1f;
    }

    public void PlayAudio(AudioStream stream, float volume = 1,bool focusing = false)
    {
        if (stream == null)
        {
            GD.PrintErr("音频流为空");
            return;
        }

        if (focusing)
        {
            foreach(var player in players)
            {
                fcSavedVolumes.Add(player.Value, volume);
                player.Value.VolumeLinear = 0;
            }
            FightBgMusic.Instance.SetTargetVolume(0);

            GetTree().CreateTimer(stream.GetLength()).Timeout += () =>
            {
                foreach (var fcSavedVolume in fcSavedVolumes)
                {
                    fcSavedVolume.Key.VolumeLinear = fcSavedVolume.Value;
                }
                FightBgMusic.Instance.SetTargetVolume(1);
                fcSavedVolumes.Clear();
            };
        }

        // 如果该音频流正在播放，直接修改音量并重新播放
        if (playingStreams.Contains(stream))
        {
            players[stream].VolumeDb = Mathf.LinearToDb(volume);
            players[stream].Play();
        }
        else
        {
            // 创建新的播放器并记录
            var player = new AudioStreamPlayer
            {
                Stream = stream,
                MaxPolyphony = 20,
                VolumeDb = Mathf.LinearToDb(volume)
            };
            players[stream] = player;
            AddChild(player);
            playingStreams.Add(stream);
            player.Play();

            // 播放结束的回调（只添加一次）
            player.Finished += () =>
            {
                playingStreams.Remove(stream);
                soundCounters[stream]--;
            };
        }

        // 计数（确保有键）
        soundCounters.TryAdd(stream, 0);
        soundCounters[stream]++;
    }
}
