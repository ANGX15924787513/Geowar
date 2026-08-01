extends Line2D
@onready var ani := $AnimationPlayer

func _ready() -> void:
	ani.play("shake_heal")
	default_color = Color.GREEN
	SignalManager.OnPlayerHealthChanged.connect(refresh)
	
func refresh(change:int,hp:int,max_hp:int):
	if change != 0:
		ani.play("shake_hurt" if change < 0 else "shake_heal")
		var new_color = Color.RED.lerp(Color.GREEN,float(hp)/max_hp)
		var tw := get_tree().create_tween();
		tw.tween_property(self,"default_color",new_color,0.3)
		