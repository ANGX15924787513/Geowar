extends Button

var count:int = 0

func _on_pressed() -> void:
	count += 1;
	if(count == 1):
		text = "你的进度会永远离开你!(真的很久! 再按一次确认)"
		modulate.lightened(0.2)
		var yesTimer := get_tree().create_timer(1)
		yesTimer.timeout.connect(on_timeout)
	if(count >= 2):
		DataManager.ClearData()
		get_parent().RefreshDisplay()
		count = 0
		text = "清除成功"
		
func on_timeout():
	count = 0
	text = "清除数据"
	modulate.darkened(0.2)