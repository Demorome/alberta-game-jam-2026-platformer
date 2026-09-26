class_name LethalComponent
extends Area2D
## Touch kills player.

signal player_touched(player: Node2D)
signal destroyed

# TODO: player joins group.
const PLAYER_GROUP := &"player"
# TODO: carriables join group.
const CARRIABLE_GROUP := &"carriable"

## Free parent on hit.
@export var free_parent_on_hit := true


func _ready() -> void:
	body_entered.connect(_on_hit)
	area_entered.connect(_on_hit)


func _on_hit(other: Node2D) -> void:
	if other.is_in_group(PLAYER_GROUP):
		player_touched.emit(other)
		_kill_player(other)
	elif other.is_in_group(CARRIABLE_GROUP):
		destroyed.emit()
		if free_parent_on_hit:
			get_parent().queue_free()


func _kill_player(player: Node2D) -> void:
	# GDScript or C# name.
	if player.has_method("die"):
		player.call("die")
	elif player.has_method("Die"):
		player.call("Die")
