extends Node2D

var animationPlayer :AnimationPlayer
var _is_in_shop :bool

func _ready() -> void:
	_is_in_shop = false
	animationPlayer = $AnimationPlayer
	$VBoxContainer/FlowContainer3/CheckBox.button_pressed = DataManager.GetBoolConfig("hide_mouse_playing")
	$VBoxContainer/FlowContainer4/HSlider.value = GlobalAudioPlayer.PlayerAttackVolume
	$VBoxContainer/FlowContainer5/CheckBox.button_pressed = DataManager.GetBoolConfig("glow_disabled")

func _on_button_pressed() -> void:
	if animationPlayer.is_playing(): return;
	GlobalAudioPlayer.PlayAudio(GlobalAudioPlayer.buttonClickSound, 1.0)
	if(_is_in_shop):
		animationPlayer.play("exit_setting")
	else:
		animationPlayer.play("enter_setting")
	_is_in_shop = not _is_in_shop


func _on_hide_mouse_checkbox_toggled(toggled_on: bool) -> void:
	DataManager.SetBoolConfig("hide_mouse_playing",toggled_on)


func _on_player_attack_volume_changed(value: float) -> void:
	GlobalAudioPlayer.PlayerAttackVolume = value
	DataManager.SetFloatConfig("player_attack_volume",value);


func _on_glow_check_box_toggled(toggled_on: bool) -> void:
	DataManager.SetBoolConfig("glow_disabled",toggled_on)
	GameManager.glowDisabled  = toggled_on
	GameManager.RefreshGlow()
