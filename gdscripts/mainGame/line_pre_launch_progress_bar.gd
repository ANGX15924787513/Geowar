extends ProgressBar
var to:float

func _process(delta: float) -> void:
	if Input.is_action_pressed("pre_launch_bullet"):
		to = 100
	else:
		to = 0
	value = lerp(value,to,0.08)