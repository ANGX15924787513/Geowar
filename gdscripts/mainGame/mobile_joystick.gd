extends CanvasLayer


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if(OS.get_name() == "Android" || OS.get_name() == "iOS"):
		visible = true
	else:
		visible = false
		process_mode = PROCESS_MODE_DISABLED
		
func _process(_delta: float) -> void:
	visible = true if GameManager.gameState == 2 else false
