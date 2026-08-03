extends Control

@onready var button := $charChooseCard/Button

func _ready() -> void:
	if(name == "charChooseCard2" && DataManager.GetUpgradeLevel("line") || 
	name == "charChooseCard3" && DataManager.GetUpgradeLevel("polygon") || 
	name == "charChooseCard"): 
		button.disabled = false
	else:
		button.disabled = true
	if(button.disabled):
		$charChooseCard/descriptionLabel.text += "\n-在商店中解锁-"