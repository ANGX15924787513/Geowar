extends SpinBox

func _ready() -> void:
	var saved = DataManager.config.get_value("settings", "fps", 0)
	Engine.max_fps = saved if saved >= 20 else 0
	value = saved if saved >= 20 else 0

func _on_value_changed(val: float) -> void:
	var fps: int = int(val)
	Engine.max_fps = fps
	DataManager.config.set_value("settings", "fps", fps)
	DataManager.Save()
