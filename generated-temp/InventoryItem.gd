extends Control

@export var grid_size: int = 50
@export var item_width: int = 1
@export var item_height: int = 1
@export var item_name: String = "Item"

var is_dragging: bool = false
var drag_offset: Vector2 = Vector2.ZERO
var rotation_angle: int = 0  # 0, 90, 180, 270

@onready var panel = $Panel
@onready var rotation_label = $RotationLabel
@onready var label = $Panel/Label

func _ready() -> void:
	custom_minimum_size = Vector2(grid_size * item_width, grid_size * item_height)
	panel.custom_minimum_size = custom_minimum_size
	label.text = item_name
	gui_input.connect(_on_gui_input)
	update_display()

func _on_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.pressed:
			if event.button_index == MOUSE_BUTTON_LEFT:
				is_dragging = true
				drag_offset = get_local_mouse_position()
				get_parent().move_child(self, -1)  # Bring to front
				get_tree().set_input_as_handled()
			elif event.button_index == MOUSE_BUTTON_RIGHT:
				rotate_item()
				get_tree().set_input_as_handled()
		else:
			if event.button_index == MOUSE_BUTTON_LEFT:
				is_dragging = false
				snap_to_grid()
				get_tree().set_input_as_handled()

func _input(event: InputEvent) -> void:
	if is_dragging and event is InputEventMouseMotion:
		var mouse_pos = get_global_mouse_position()
		var parent_pos = get_parent().get_global_rect().position
		position = mouse_pos - parent_pos - drag_offset
		get_tree().set_input_as_handled()

func rotate_item() -> void:
	rotation_angle = (rotation_angle + 90) % 360
	
	# Swap dimensions for 90 and 270 degree rotations
	if rotation_angle % 180 == 90:
		var temp = item_width
		item_width = item_height
		item_height = temp
	
	custom_minimum_size = Vector2(grid_size * item_width, grid_size * item_height)
	panel.custom_minimum_size = custom_minimum_size
	update_display()

func snap_to_grid() -> void:
	var grid_pos = position.round() / grid_size * grid_size
	position = grid_pos
	
	# Clamp to parent bounds
	var parent = get_parent()
	if parent:
		var max_x = parent.size.x - size.x
		var max_y = parent.size.y - size.y
		grid_pos.x = clamp(grid_pos.x, 0, max_x)
		grid_pos.y = clamp(grid_pos.y, 0, max_y)
		position = grid_pos

func update_display() -> void:
	rotation_label.text = str(rotation_angle) + "°"
