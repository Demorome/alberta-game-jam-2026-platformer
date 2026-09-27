extends Node2D

# @export var target_level: String = ""
@export var target_scene: PackedScene

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	# Every level shows HUD
	GameManager.show_hud()

func _on_body_entered(body: Node2D) -> void:
	if target_scene:
		get_tree().change_scene_to_packed.call_deferred(target_scene)
	else:
		push_warning("Target scene is not assigned on " + name)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
