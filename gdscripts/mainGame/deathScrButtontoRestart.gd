extends Button


func restart():
	GlobalAudioPlayer.PlayAudio(GlobalAudioPlayer.buttonClickSound, 1.0)
	EasyTransition.transition_to("res://scenes/main_game.tscn")
