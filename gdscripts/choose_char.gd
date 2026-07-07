extends Control

func destroy():
	queue_free()
	
func _ready() -> void:
	if GameManager.charChose == true:
		destroy()
		return
	GameManager.gameState = 1
	
func emit_card_out():
	SignalManager.OnCardOut.emit()
