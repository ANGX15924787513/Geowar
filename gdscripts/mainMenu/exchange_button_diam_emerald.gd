extends Button
@export var line_edit: LineEdit
@export var emer_label: Label
signal on_diam_emerald(count :int)

func _on_pressed() -> void:
	if DataManager.Diamond < int(line_edit.text) * 10:
		emer_label.on_not_enough()
		return
	on_diam_emerald.emit(int(line_edit.text))
