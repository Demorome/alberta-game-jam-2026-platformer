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
# @export var score_value := 100


func _ready() -> void:
	if not monitoring:
		monitoring = true


func _process(_delta: float) -> void:
	# Manually getting the bags to detect it because of a weird bug if carrying it.
	var bags = get_tree().get_nodes_in_group(BAG_GROUP)
	
	# Don't ask me exactly how this works!
	for bag in bags:
		if not bag.has_meta("lethal_hit"):
			var distance = global_position.distance_to(bag.global_position)
			if distance < 40:
				bag.set_meta("lethal_hit", true)
				_on_hit(bag)
				await get_tree().create_timer(0.15).timeout
				if is_instance_valid(bag):
					bag.remove_meta("lethal_hit")


func _on_hit(other: Node2D) -> void:
	if other.get_parent() and other.get_parent().is_in_group(PLAYER_GROUP):
		player_touched.emit(other.get_parent())
		_kill_player(other.get_parent())
	elif other.is_in_group(BAG_GROUP):
		destroyed.emit()
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
