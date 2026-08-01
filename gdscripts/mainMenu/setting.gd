extends Node2D

var animationPlayer :AnimationPlayer
var _is_in_shop :bool

func _ready() -> void:
	_is_in_shop = false
	animationPlayer = $AnimationPlayer
	$VBoxContainer/FlowContainer3/CheckBox.button_pressed = DataManager.GetBoolConfig("hide_mouse_playing")

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
