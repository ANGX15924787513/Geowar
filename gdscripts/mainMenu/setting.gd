extends Control

var animationPlayer :AnimationPlayer
var _is_in_shop :bool

func _ready() -> void:
	_is_in_shop = false
	animationPlayer = $AnimationPlayer
	$VBoxContainer/FlowContainer3/CheckBox.button_pressed = DataManager.GetBoolConfig("hide_mouse_playing")
	$VBoxContainer/FlowContainer6/CheckBox.button_pressed = DataManager.GetBoolConfig("use_custom_cursor")
	Input.set_custom_mouse_cursor(load("res://texture/UICursor/tile_0028.png") if $VBoxContainer/FlowContainer6/CheckBox.button_pressed else null)
	$VBoxContainer/FlowContainer4/HSlider.value = GlobalAudioPlayer.PlayerAttackVolume
	$VBoxContainer/FlowContainer5/CheckBox.button_pressed = DataManager.GetBoolConfig("glow_disabled")
	$VBoxContainer/mainMenuBgMusicVol/HSlider.value = GlobalAudioPlayer.menuBgVolume
	$VBoxContainer/GamingBgMusicVol/HSlider.value = GlobalAudioPlayer.gamingBgVolume
	$VBoxContainer/GamingRand/CheckBox.button_pressed = DataManager.GetBoolConfig("gaming_rand_play")

func _on_button_pressed() -> void:
	if animationPlayer.is_playing(): return;
	GlobalAudioPlayer.PlayAudio(GlobalAudioPlayer.buttonClickSound, 1.0)
	if(_is_in_shop):
		animationPlayer.play("exit_setting")
	else:
		animationPlayer.play("enter_setting")
	_is_in_shop = not _is_in_shop

func _on_using_custom_cursor_toggled(toggled_on: bool) -> void:
	Input.set_custom_mouse_cursor(load("res://texture/UICursor/tile_0028.png") if toggled_on else null)
	DataManager.SetBoolConfig("use_custom_cursor",toggled_on)

func _on_hide_mouse_checkbox_toggled(toggled_on: bool) -> void:
	DataManager.SetBoolConfig("hide_mouse_playing",toggled_on)

#---音量设置---
#在这里添加方法  -> _ready()初始化 -> 在globalAudioPlayer里加变量 -> GlobalAudioManager.VolumeInit -> 实现
func _on_player_attack_volume_changed(value: float) -> void:
	GlobalAudioPlayer.PlayerAttackVolume = value
	DataManager.SetFloatConfig("player_attack_volume",value);
	
func _on_mainmenu_bg_vol_slider_value_changed(value: float) -> void:
	GlobalAudioPlayer.menuBgVolume = value
	DataManager.SetFloatConfig("mainmenu_bg_volume",value);
	
func _on_gaming_bg_vol_slider_value_changed(value: float) -> void:
	GlobalAudioPlayer.gamingBgVolume = value
	DataManager.SetFloatConfig("gaming_bg_volume",value);
	
func _on_gaming_rand_check_box_toggled(toggled_on: bool) -> void:
	DataManager.SetBoolConfig("gaming_rand_play",toggled_on)
#/---音量设置---

func _on_glow_check_box_toggled(toggled_on: bool) -> void:
	DataManager.SetBoolConfig("glow_disabled",toggled_on)
	GameManager.glowDisabled  = toggled_on
	GameManager.RefreshGlow()
