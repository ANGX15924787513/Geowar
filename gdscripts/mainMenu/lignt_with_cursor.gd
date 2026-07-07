extends RigidBody2D
@export_range(0,20) var pull_strenth :float

func _process(_delta: float) -> void:
	var to_pos := get_global_mouse_position() - global_position
	linear_velocity = to_pos * pull_strenth
