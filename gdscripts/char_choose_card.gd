extends Control
@export var showIcon: PackedScene

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if showIcon == null:
		return
	var icon: Node = showIcon.instantiate()
	var iconAni:AnimationPlayer = icon.get_node("AnimationPlayer")
	iconAni.play("in_card_show")
	$charChooseCard/Icon.add_child(icon)
