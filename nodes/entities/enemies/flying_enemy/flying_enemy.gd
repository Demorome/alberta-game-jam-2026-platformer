extends ShootEnemy
## Shoot enemy, flying.


func _ready() -> void:
	# Keep shooter setup.
	super()
	print("🪰 Flying enemy alive")
	
func _shoot() -> void:
	super()
	$AnimatedSprite2D.play(&"shooting")
