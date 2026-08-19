extends Timer


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	GameManager.OnGameStateChange.connect(on_game_state_change)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func on_game_state_change(new_state:int):
	if new_state == 2:#GAMING
		set_process_mode(PROCESS_MODE_INHERIT)
	else:
		set_process_mode(PROCESS_MODE_DISABLED)