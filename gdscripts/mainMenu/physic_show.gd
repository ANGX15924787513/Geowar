extends HBoxContainer

@export var count: int = 3
@export var itemTextures: Array[Texture2D] = []

func _ready() -> void:
	if itemTextures.is_empty():
		return

	var loaded: Array[Texture2D] = []
	for tex in itemTextures:
		if tex != null:
			loaded.append(tex)

	for i in range(count):
		create_item(loaded.pick_random())

func create_item(texture: Texture2D) -> void:
	var file_name := texture.resource_path.get_file().get_basename()

	var vbox := VBoxContainer.new()
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER

	var rect := TextureRect.new()
	rect.texture = texture
	rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	rect.custom_minimum_size = Vector2(64, 64)
	rect.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	vbox.add_child(rect)

	var label := Label.new()
	label.text = file_name.substr(0,13) + "..." if file_name.length() > 15 else file_name
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.add_theme_font_size_override("font_size", 21)
	label.add_theme_font_override("font", load("res://fonts/BoutiqueBitmap9x9_1.9.ttf"))
	vbox.add_child(label)

	add_child(vbox)
