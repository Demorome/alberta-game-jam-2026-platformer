extends Node2D

@export var push_force: float = 400.0

func _ready() -> void:
	pass # Replace with function body.

func _process(delta: float) -> void:
	#$TestPlayer/ChainSystem.position = get_global_mouse_position()
	pass

func _input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		
		#var direction = (get_global_mouse_position() - $Bag.global_position).normalized()
		var direction = ($TestPlayer/ChainSystem.position - $TestPlayer/Bag.global_position).normalized()
		var impulse = direction * push_force
		$TestPlayer/Bag.apply_central_impulse(impulse + (Vector2.UP * ($TestPlayer/ChainSystem.position.distance_to($TestPlayer/Bag.position) * 1.5)))
