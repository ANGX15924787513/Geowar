extends Camera2D
@onready var followObj: Node2D = $"../player"

func switch_to_center_player():
	anchor_mode = ANCHOR_MODE_DRAG_CENTER

func _process(delta: float) -> void:
	global_position = followObj.global_position
