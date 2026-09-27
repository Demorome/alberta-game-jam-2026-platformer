extends Area2D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_body_entered(body: Node2D) -> void:
	if body.get_parent().is_in_group(&"player_group"):
		_kill_player(body.get_parent())

func _kill_player(player: Node2D) -> void:
	GameManager.add_death()
	# GDScript or C# name.
	if player.has_method("die"):
		player.call("die")
	elif player.has_method("Die"):
		player.call("Die")
	else:
		# No die yet, restart.
		GameManager.restart_level()
