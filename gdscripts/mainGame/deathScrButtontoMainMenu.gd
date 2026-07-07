extends Button


# Called when the node enters the scene tree for the first time.
func switch_to_main_menu():
	GlobalAudioPlayer.PlayAudio(GlobalAudioPlayer.buttonClickSound, 1.0)
	EasyTransition.transition_to("res://scenes/main_menu.tscn",1,EasyTransition.TransitionAnim.FADE)
