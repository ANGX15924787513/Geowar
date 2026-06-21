extends Control

func destroy():
	queue_free()
	
func _ready() -> void:
	if GameManager.charChose == true:
		destroy()
