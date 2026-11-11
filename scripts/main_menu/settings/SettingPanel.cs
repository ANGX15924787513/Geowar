using Godot;

public partial class SettingPanel : Control
{
    public static SettingPanel Instance;
    [Export]private AnimationPlayer animationPlayer;
    private bool isInSetting;

    public override void _Ready()
    {
        window = GetWindow();
        fullRes = new Vector2I(1152,648);
        GetNode<CheckBox>("VBoxContainer/FlowContainer3/CheckBox").ButtonPressed = DataManager.Instance.GetBoolConfig("hide_mouse_playing");
        GetNode<CheckBox>("VBoxContainer/FlowContainer6/CheckBox").ButtonPressed = DataManager.Instance.GetBoolConfig("use_custom_cursor");
        Input.SetCustomMouseCursor(GetNode<CheckBox>("VBoxContainer/FlowContainer6/CheckBox").ButtonPressed ? GD.Load<Texture2D>("res://texture/UICursor/tile_0028.png") : null);
        GetNode<HSlider>("VBoxContainer/FlowContainer4/HSlider").Value = GlobalAudioPlayer.Instance.PlayerAttackVolume;
        GetNode<CheckBox>("VBoxContainer/FlowContainer5/CheckBox").ButtonPressed = DataManager.Instance.GetBoolConfig("glow_disabled");
        GetNode<HSlider>("VBoxContainer/mainMenuBgMusicVol/HSlider").Value = GlobalAudioPlayer.Instance.menuBgVolume;
        GetNode<HSlider>("VBoxContainer/GamingBgMusicVol/HSlider").Value = GlobalAudioPlayer.Instance.gamingBgVolume;
        GetNode<CheckBox>("VBoxContainer/GamingRand/CheckBox").ButtonPressed = DataManager.Instance.GetBoolConfig("gaming_rand_play");
        GetNode<CheckBox>("VBoxContainer/FlowContainer7/CheckBox").ButtonPressed = DataManager.Instance.GetBoolConfig("skip_starting");
        var resOption = GetNode<OptionButton>("VBoxContainer/FlowContainer8/SpinBox");
        resOption.Select(DataManager.Instance.GetIntConfig("resolution"));
        SetResolutionIndex(resOption.Selected);
    }
    
    public override void _EnterTree() => Instance = this;
    public override void _ExitTree() => Instance = null;

    private void _on_button_pressed()
    {
        if (animationPlayer.IsPlaying()) return;
        GlobalAudioPlayer.Instance.PlayAudio(GlobalAudioPlayer.Instance.buttonClickSound);
        animationPlayer.Play(isInSetting ? "exit_setting" : "enter_setting");
        isInSetting = !isInSetting;
        MainMenuShop.Instance.Visible = !isInSetting;
        MainMenuRoot.Instance.SwitchCentreVisible(!isInSetting);
    }

    private void _on_using_custom_cursor_toggled(bool toggled_on)
    {
        Input.SetCustomMouseCursor(toggled_on ? GD.Load<Texture2D>("res://texture/UICursor/tile_0028.png") : null);
        DataManager.Instance.SetBoolConfig("use_custom_cursor", toggled_on);
    }

    private void _on_hide_mouse_checkbox_toggled(bool toggled_on)
    {
        DataManager.Instance.SetBoolConfig("hide_mouse_playing", toggled_on);
    }

    //---音量设置---
    //在这里添加方法  -> _ready()初始化 -> 在globalAudioPlayer里加变量 -> GlobalAudioManager.VolumeInit -> 实现
    //拖动期间只改内存(实时生效), 松手才写存档
    private void _on_player_attack_volume_changed(float value)
    {
        GlobalAudioPlayer.Instance.PlayerAttackVolume = value;
    }

    private void _on_player_attack_volume_drag_ended(bool valueChanged)
    {
        if (!valueChanged) return;
        DataManager.Instance.SetFloatConfig("player_attack_volume", GlobalAudioPlayer.Instance.PlayerAttackVolume);
    }

    private void _on_mainmenu_bg_vol_slider_value_changed(float value)
    {
        GlobalAudioPlayer.Instance.menuBgVolume = value;
    }

    private void _on_mainmenu_bg_vol_slider_drag_ended(bool valueChanged)
    {
        if (!valueChanged) return;
        DataManager.Instance.SetFloatConfig("mainmenu_bg_volume", GlobalAudioPlayer.Instance.menuBgVolume);
    }

    private void _on_gaming_bg_vol_slider_value_changed(float value)
    {
        GlobalAudioPlayer.Instance.gamingBgVolume = value;
    }

    private void _on_gaming_bg_vol_slider_drag_ended(bool valueChanged)
    {
        if (!valueChanged) return;
        DataManager.Instance.SetFloatConfig("gaming_bg_volume", GlobalAudioPlayer.Instance.gamingBgVolume);
    }

    private void _on_gaming_rand_check_box_toggled(bool toggled_on)
    {
        DataManager.Instance.SetBoolConfig("gaming_rand_play", toggled_on);
    }
    //---音量设置---

    private void _on_glow_check_box_toggled(bool toggled_on)
    {
        DataManager.Instance.SetBoolConfig("glow_disabled", toggled_on);
        GameManager.Instance.glowDisabled = toggled_on;
        GameManager.Instance.RefreshGlow();
    }

    private void on_skip_starting_check_box_toggled(bool toggled_on)
    {
        DataManager.Instance.SetBoolConfig("skip_starting", toggled_on);
    }

    private Vector2I fullRes;
    private Window window;
    private void on_resolution_option_button_selected(int index)
    {
        SetResolutionIndex(index);
        DataManager.Instance.SetIntConfig("resolution", index);
    }

    private void SetResolutionIndex(int index)
    {
        GetTree().Root.ContentScaleSize = index switch
        {
            0 => fullRes,
            1 => fullRes / 2,
            2 => fullRes / 4,
            _ => GetTree().Root.ContentScaleSize
        };
        GetTree().Root.ContentScaleFactor = index switch
        {
            0 => 1,
            1 => 0.5f,
            2 => 0.25f,
            _ => GetTree().Root.ContentScaleFactor
        };
    }
}