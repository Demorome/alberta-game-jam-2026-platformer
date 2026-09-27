class_name ShootEnemy
extends StaticBody2D
## Shoots on interval a set interval and may or may not need to "see" the player.

const BULLET_SCENE := preload("res://nodes/entities/enemies/bullet/fireball.tscn")

## Flag to tell if the enemy sould shoot only when player is in sight.
@export var use_sight := true
## Should the enemy flip if player passes it?
@export var face_player := false
## Wait before first shot.
@export_range(0.0, 5.0, 0.05, "suffix:s") var shoot_delay := 0.5
## Time between shots.
@export_range(0.1, 10.0, 0.1, "suffix:s") var shoot_interval := 1.5
## Pixels per second.
@export var bullet_speed := 300.0
## Initial bullet direction.
@export var bullet_direction := Vector2.LEFT
## Delay in ms before spawning the bullet after animation starts.
@export_range(0, 2000, 10, "suffix:ms") var bullet_spawn_delay := 100

var _facing_right := false

@onready var placeholder: Polygon2D = $Placeholder
@onready var animatedSprite: AnimatedSprite2D = $AnimatedSprite2D
@onready var muzzle: Marker2D = $Muzzle
@onready var shoot_timer: Timer = $ShootTimer
@onready var detection_area: Area2D = $DetectionArea
@onready var sight_shape: CollisionShape2D = $DetectionArea/CollisionShape2D
@onready var deathSound: AudioStreamPlayer = $deathSound

func _ready() -> void:
	shoot_timer.timeout.connect(_on_shoot_timer_timeout)
	detection_area.body_entered.connect(_on_body_entered)
	detection_area.body_exited.connect(_on_body_exited)
	# Always shoot if not using sight set.
	if not use_sight:
		shoot_timer.start(shoot_delay)


func _physics_process(_delta: float) -> void:
	if face_player:
		_update_facing()


func _update_facing() -> void:
	var player := get_tree().get_first_node_in_group(LethalComponent.PLAYER_GROUP) as Node2D
	if player == null:
		return
	var should_face_right := player.global_position.x > global_position.x
	if should_face_right != _facing_right:
		_facing_right = should_face_right
		_mirror()


func _mirror() -> void:
	# Flip children horizontally.
	#placeholder.scale.x = -placeholder.scale.x
	animatedSprite.flip_h = not animatedSprite.flip_h
	muzzle.position.x = -muzzle.position.x
	sight_shape.position.x = -sight_shape.position.x


func _on_body_entered(body: Node2D) -> void:
	# print("[SHOOT_ENEMY] ↳ body entered: ", body)
	# if use_sight and body.is_in_group(LethalComponent.PLAYER_GROUP):
	if use_sight: # Realistically, only player related bodies are in the mask layer
		# print("[SHOOT_ENEMY] 👀 player entered sight, starting shoot timer")
		shoot_timer.start(shoot_delay)


func _on_body_exited(body: Node2D) -> void:
	# print("[SHOOT_ENEMY] ↳ body exited: ", body)
	# print("[SHOOT_ENEMY] body.get_collision_layer_value(1): ", body.get_collision_layer_value(1))
	# print("[SHOOT_ENEMY] body.get_collision_layer_value(2): ", body.get_collision_layer_value(2))
	# if use_sight and body.is_in_group(LethalComponent.PLAYER_GROUP):
	if use_sight: # Realistically, only player related bodies are in the mask layer
		# print("[SHOOT_ENEMY] 👀 player exited sight, stopping shoot timer")
		shoot_timer.stop()


func _on_shoot_timer_timeout() -> void:
	_shoot()
	shoot_timer.start(shoot_interval)


func _shoot() -> void:
	var direction := bullet_direction.normalized()
	if _facing_right:
		direction.x = -direction.x

	animatedSprite.play(&"shooting")

	# Wait for bullet spawn delay
	if bullet_spawn_delay > 0:
		await get_tree().create_timer(bullet_spawn_delay / 1000.0).timeout

	var bullet: EnemyBullet = BULLET_SCENE.instantiate()
	bullet.direction = direction
	bullet.speed = bullet_speed
	# Level owns bullet.
	get_parent().add_child(bullet)
	bullet.global_position = muzzle.global_position


func _on_animated_sprite_2d_animation_finished() -> void:
	if animatedSprite.animation == "shooting":
		animatedSprite.play("default")

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
