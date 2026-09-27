class_name EnemyBullet
extends Node2D
## Flies straight forward.

# Shooter sets these when instantiating the bullet.
var speed := 300.0
var direction := Vector2.LEFT
@onready var deathSound: AudioStreamPlayer = $deathSound

func _ready() -> void:
	$VisibleOnScreenNotifier2D.screen_exited.connect(queue_free)
	#$Sprite2D.rotate(direction.angle())
	#$Sprite2D.flip_h = direction.x < 0;
	#$Sprite2D.flip_h = direction.y > 0;
	if  direction.y > 0:
		$Sprite2D.rotate(direction.angle() + deg_to_rad(180))
		


func _physics_process(delta: float) -> void:
	position += direction * speed * delta
	# TODO: Destroys when hitting ground?


func die():
	# Play death sound if available
	if deathSound.stream != null:
		deathSound.play()
	self.white_and_fade_out(0, 0.1)

func white_and_fade_out(flash_duration: float = 0.15, fade_duration: float = 0.5) -> void:
	# 1. Create a Tween instance
	var tween = create_tween()
	
	# 2. Flash to solid white by over-saturating the RGB values (Color channels higher than 1.0 will over-saturate the sprite texture to white)
	var white_flash = Color(10.0, 10.0, 10.0, 1.0)
	tween.tween_property(self, "modulate", white_flash, flash_duration)
	
	# 3. Fade out smoothly to completely transparent
	var transparent = Color(10.0, 10.0, 10.0, 0.0)
	tween.tween_property(self, "modulate", transparent, fade_duration)
	
	# 4. Automatically remove or hide the node when finished
	tween.finished.connect(queue_free) 
