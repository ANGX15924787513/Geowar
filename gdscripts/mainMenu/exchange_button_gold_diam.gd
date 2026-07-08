extends Button
@export var line_edit: LineEdit
@export var diamond_label: Label
signal on_gold_diam(count :int)

func _on_pressed() -> void:
	if DataManager.Gold < int(line_edit.text) * 10:
		diamond_label.on_not_enough()
		return
	on_gold_diam.emit(int(line_edit.text))
