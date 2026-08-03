extends Node2D
@export var main_menu :PackedScene

func _ready() -> void:
	GameManager.gameState = 5
	$WorldEnvironment.environment.glow_enabled = not GameManager.glowDisabled;

func change_to_menu():
	EasyTransition.transition_to(main_menu.resource_path,0.5,EasyTransition.TransitionAnim.BLUR)
