extends Node2D

@export var push_force: float = 400.0

func _ready() -> void:
	pass # Replace with function body.

func _process(delta: float) -> void:
	$ChainSystem.position = get_global_mouse_position()
	pass

func _input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		
		var direction = (get_global_mouse_position() - $Bag.global_position).normalized()
		var impulse = direction * push_force
		$Bag.apply_central_impulse(impulse + (Vector2.UP * (get_global_mouse_position().distance_to($Bag.position) * 1.5)))
