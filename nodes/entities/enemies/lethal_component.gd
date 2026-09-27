class_name LethalComponent
extends Area2D
## Touch kills player.

signal player_touched(player: Node2D)
signal destroyed

# TODO: player joins group.
const PLAYER_GROUP := &"player_group"
# TODO: carriables join group.
const BAG_GROUP := &"bag_group"

## Free parent on hit.
@export var free_parent_on_hit := true
## Points earned when enemy destroyed.
@export var score_value := 100


func _ready() -> void:
	body_entered.connect(_on_hit)
	area_entered.connect(_on_hit)


func _on_hit(other: Node2D) -> void:
	print("💥 [Lethal] Hit by: ", other)
	print("💥 [Lethal] Parent of the hitter: ", other.get_parent())
	if other.get_parent().is_in_group(PLAYER_GROUP):
		print("💥 [Lethal] Player hit: ", other)
		player_touched.emit(other.get_parent())
		_kill_player(other.get_parent())
	elif other.is_in_group(BAG_GROUP):
		print("💥 [Lethal] Carriable hit: ", other)
		destroyed.emit()
		GameManager.add_score(score_value)
		if free_parent_on_hit:
			var parent = get_parent()
			if parent.has_method("die"):
				parent.call("die")
			else:
				parent.queue_free()


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
