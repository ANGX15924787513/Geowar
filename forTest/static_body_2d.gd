extends CharacterBody2D

func _process(delta: float) -> void:
	rotate(PI * delta)
