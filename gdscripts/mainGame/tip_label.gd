extends Label

@export var tips:PackedStringArray

func refresh_tip():
	if tips.is_empty():
		print("你没有写提示!")
		text = "Tip:" + "你没有写提示!"
	text = "Tip:\n" + tips.get(randi_range(0,tips.size() - 1))
