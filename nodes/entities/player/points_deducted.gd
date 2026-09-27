extends Node2D
## Flying text that fades out, to show points deducted

# How to use:
# 	var points = preload("res://nodes/entities/player/points_deducted.tscn").instantiate()
# 	points.label.text = "-50"
# 	points.global_position = player.global_position
# 	add_child(points)

@onready var label: Label = $Label


func _ready() -> void:
	
	var tween = create_tween()
	
	# Move up 20 pixels in 0.5 seconds
	tween.tween_property(self, "position", position + Vector2(0, -20), 0.5)
	
	# Wait another 0.5 seconds before fading out
	await get_tree().create_timer(0.5).timeout
	
	# Fade out over 0.3 seconds
	var fade_tween = create_tween()
	fade_tween.tween_property(label, "modulate:a", 0.0, 0.3)
	await fade_tween.finished
	
	queue_free()
