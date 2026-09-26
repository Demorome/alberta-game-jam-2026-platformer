class_name PacifistEnemy
extends StaticBody2D
## Peaceful standing enemy.

@onready var animated_sprite: AnimatedSprite2D = $AnimatedSprite2D


func _ready() -> void:
	# Play idle animation.
	animated_sprite.play(&"default")
