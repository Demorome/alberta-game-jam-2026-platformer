class_name EnemyBullet
extends Node2D
## Flies straight forward.

# Shooter sets these when instantiating the bullet.
var speed := 300.0
var direction := Vector2.LEFT


func _ready() -> void:
	$VisibleOnScreenNotifier2D.screen_exited.connect(queue_free)
	$Sprite2D.rotate(direction.angle())
	$Sprite2D.flip_h = direction.x < 0;


func _physics_process(delta: float) -> void:
	position += direction * speed * delta
	# TODO: Destroys when hitting ground?
