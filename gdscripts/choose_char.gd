extends Node2D
@export var camera: PackedScene

func destroy():
	queue_free()

func summon_camera():
	get_tree().current_scene.add_child(camera.instantiate())
	
func _ready() -> void:
	if GameManager.charChose == true:
		destroy()
