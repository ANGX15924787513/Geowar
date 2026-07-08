extends HBoxContainer
@export var count: int = 3

var textures: Array = []
var file_paths: Array = []
var loaded_set: Dictionary = {}

func _ready() -> void:
	var path = "res://texture/MCTexture/main_menu/item/"
	var dir: DirAccess = DirAccess.open(path)
	if dir == null:
		push_error("无法打开目录")
		return
	
	dir.list_dir_begin()
	var file_name: String = dir.get_next()
	
	while file_name != "":
		if file_name.ends_with(".png") and not file_name.ends_with(".import"):
			var full_path = path + file_name
			file_paths.append(full_path)
			# 请求线程加载
			ResourceLoader.load_threaded_request(full_path, "Texture2D")
		file_name = dir.get_next()
	dir.list_dir_end()
	
	print("找到 %d 个文件，线程加载中..." % file_paths.size())

func _process(_delta: float) -> void:
	if file_paths.is_empty():
		return
	
	# 检查已加载完成的
	var completed = []
	for file_path in file_paths:
		var status = ResourceLoader.load_threaded_get_status(file_path)
		if status == ResourceLoader.THREAD_LOAD_LOADED:
			var texture = ResourceLoader.load_threaded_get(file_path)
			if texture is Texture2D:
				textures.append(texture)
			completed.append(file_path)
	
	# 移除已完成的
	for path in completed:
		file_paths.erase(path)
	
	# 全部加载完成
	if file_paths.is_empty():
		print("全部加载完成！共 %d 个纹理" % textures.size())
		for i in range(count):
			create_texture_rect()
		set_process(false)  # 停止检查

func create_texture_rect() -> void:
	if textures.is_empty():
		return
	
	# 随机选一个纹理
	var texture: Texture2D = textures.pick_random()
	
	# 获取文件名（不含后缀）
	var file_name: String = texture.resource_path.get_file().get_basename()
	
	# 外层容器
	var vbox = VBoxContainer.new()
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	
	# TextureRect
	var texture_rect = TextureRect.new()
	texture_rect.texture = texture
	texture_rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	texture_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	texture_rect.custom_minimum_size = Vector2(64, 64)
	texture_rect.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	vbox.add_child(texture_rect)
	
	# 文件名标签
	var label = Label.new()
	label.text = file_name
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.add_theme_font_size_override("font_size", 21)
	label.add_theme_font_override("font",load("res://fonts/BoutiqueBitmap9x9_1.9.ttf"))
	vbox.add_child(label)
	
	add_child(vbox)