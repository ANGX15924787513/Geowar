extends Line2D

@export var radius: float = 80:
	set(value):
		radius = value
		update_polygon()

@export var shake_intensity: float = 12
@export var point_count: int = 50:
	set(value):
		point_count = value
		update_polygon()

@export var shake_duration: float = 0.5
@export var recover_speed: float = 3.0

var shake_timer: float = 0.0
var is_shaking: bool = false
var _point_offsets: PackedFloat32Array
var _offset_velocities: PackedFloat32Array

# 血量驱动
var _hp_ratio: float = 1.0
var _display_ratio: float = 1.0
var _display_radius: float

func _ready() -> void:
	SignalManager.OnPlayerHealthChanged.connect(on_player_got_hurt)
	_display_radius = radius
	_display_ratio = 1.0
	_init_offsets()
	update_polygon()

func _init_offsets() -> void:
	_point_offsets.resize(point_count)
	_offset_velocities.resize(point_count)

func _process(delta: float) -> void:
	# 平滑过渡 display_ratio → hp_ratio
	_display_ratio = move_toward(_display_ratio, _hp_ratio, 0.8 * delta)
	_display_radius = move_toward(_display_radius, radius * _display_ratio, radius * delta)

	# 振动逻辑
	if is_shaking:
		shake_timer -= delta
		if shake_timer <= 0:
			is_shaking = false
		_shake_update(delta)
	else:
		_recover_update(delta)

	# 每帧根据当前显示的半径和颜色更新
	_draw()

func _draw() -> void:
	var pts: PackedVector2Array
	pts.resize(point_count)
	var r: float = _display_radius
	var half_alpha_red := Color.RED;
	half_alpha_red.a = 0.7
	var half_alpha_green := Color.GREEN;
	half_alpha_green.a = 0.7
	var color: Color = lerp(half_alpha_red, half_alpha_green, _display_ratio)
	default_color = color

	for i in point_count:
		var angle: float = deg_to_rad(360.0 * i / point_count)
		pts[i] = Vector2(cos(angle), sin(angle)) * (r + _point_offsets[i])
	points = pts

func get_pos_in_circle_with_index(r: float, index: int) -> Vector2:
	var angle: float = deg_to_rad(360.0 * index / point_count)
	return Vector2(cos(angle), sin(angle)) * r

func update_polygon() -> void:
	pass

func on_player_got_hurt(_damage: int, hp: int, max_hp: int):
	_hp_ratio = clamp(float(hp) / float(max_hp), 0.0, 1.0)
	shake_timer = shake_duration
	is_shaking = true
	for i in point_count:
		_offset_velocities[i] = randf_range(-shake_intensity, shake_intensity) * 4.0

func _shake_update(delta: float) -> void:
	for i in point_count:
		var spring = -_point_offsets[i] * 8.0
		var damping = -_offset_velocities[i] * 2.5
		_offset_velocities[i] += (spring + damping) * delta
		_point_offsets[i] += _offset_velocities[i] * delta

func _recover_update(delta: float) -> void:
	for i in point_count:
		_point_offsets[i] = move_toward(_point_offsets[i], 0.0, shake_intensity * recover_speed * delta)
		_offset_velocities[i] = move_toward(_offset_velocities[i], 0.0, shake_intensity * 2.0 * delta)
