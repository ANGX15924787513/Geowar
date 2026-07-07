extends CanvasLayer
var health_label:Label

func _ready() -> void:
	health_label = $Label2
	SignalManager.OnPlayerHealthChanged.connect(on_player_health_changed)

func _process(_delta: float) -> void:
	if(GameManager.gameState != 2):
		visible = false
	else:
		visible = true

func on_player_health_changed(_damage: int,health: int,max_health: int):
	health_label.text = str(health) + "/" + str(max_health)