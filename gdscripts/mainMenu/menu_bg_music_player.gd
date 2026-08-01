extends AudioStreamPlayer
@export var bg_list :Array[AudioStream]

func play_bg():
	stream = bg_list[randi_range(0,bg_list.size() - 1)]
	play()