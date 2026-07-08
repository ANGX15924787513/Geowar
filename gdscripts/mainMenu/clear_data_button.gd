extends Button

var count:int = 0

func _on_pressed() -> void:
	count += 1;
	if(count >= 2):
		DataManager.ClearData()
		get_parent().RefreshDisplay()