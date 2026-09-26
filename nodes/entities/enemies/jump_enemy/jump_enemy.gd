extends CharacterBody2D
## Jumps on timer and an array of pattern that multiplies the speed force.

## Interval between jumps.
@export_range(0.1, 10.0, 0.1, "suffix:s") var jump_interval := 1.5
## Jump start speed or impulse force.
@export var jump_impulse := 400.0
## Delay before first jump.
@export_range(0.0, 5.0, 0.1, "suffix:s") var jump_delay := 0.0
## Pattern that multiplies the jump impulse on each iteration.
@export var jump_pattern: Array[float] = []

var _wait := 0.0
var _pattern_index := 0


func _ready() -> void:
	print("🦘 Jump enemy alive")
	print("🦘 Jump pattern: ", jump_pattern)
	_wait = jump_delay


func _physics_process(delta: float) -> void:
	if not is_on_floor():
		velocity += get_gravity() * delta
		$AnimatedSprite2D.play(&"jump")
	else:
		$AnimatedSprite2D.play(&"default")
		_wait -= delta
		if _wait <= 0.0:
			_jump()

	move_and_slide()


func _jump() -> void:
	velocity.y = -jump_impulse * _next_multiplier()
	_wait = jump_interval


func _next_multiplier() -> float:
	if jump_pattern.is_empty():
		return 1.0
	print("🦘 Jump pattern index: ", _pattern_index)
	print("🦘 Jump pattern multiplier: ", jump_pattern[_pattern_index])
	var multiplier := jump_pattern[_pattern_index]
	_pattern_index = (_pattern_index + 1) % jump_pattern.size()
	return multiplier
