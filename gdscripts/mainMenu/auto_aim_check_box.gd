extends CheckBox
func _ready() -> void:
	button_pressed = DataManager.GetBoolConfig("auto_aim")

func _on_toggled(toggled_on: bool) -> void:
	DataManager.SetBoolConfig("auto_aim",toggled_on)
