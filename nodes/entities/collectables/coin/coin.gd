class_name Coin
extends Area2D

## How much score it adds up when collected
@export var score_amount := 100

@onready var animated_sprite: AnimatedSprite2D = $AnimatedSprite2D
@onready var sfx: AudioStreamPlayer = $SFX

const PLAYER_GROUP := &"player_group"

func _ready() -> void:
	body_entered.connect(_on_body_entered)


func _on_body_entered(body: Node2D) -> void:
	# Check if it's the player by group
	if body.get_parent() and body.get_parent().is_in_group(PLAYER_GROUP):
		GameManager.add_score(score_amount)
		if sfx.stream != null:
			sfx.play()
			# Destroys the sound after it is done
			await get_tree().create_timer(sfx.stream.get_length()).timeout
		queue_free()
