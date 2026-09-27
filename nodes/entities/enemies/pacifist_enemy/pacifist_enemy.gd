class_name PacifistEnemy
extends StaticBody2D
## Peaceful standing enemy.

@onready var animated_sprite: AnimatedSprite2D = $AnimatedSprite2D
@onready var deathSound: AudioStreamPlayer = $deathSound


func _ready() -> void:
	# Play idle animation.
	animated_sprite.play(&"default")

func die():
	# Play death sound if available
	if deathSound.stream != null:
		deathSound.play()
	self.white_and_fade_out(0.10, 0.15)

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
