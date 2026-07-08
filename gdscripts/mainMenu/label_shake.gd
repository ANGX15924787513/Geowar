extends Label

func on_not_enough():
	var cathe: String = text
	text = "穷"
	await get_tree().create_timer(0.5).timeout
	text = cathe
	
