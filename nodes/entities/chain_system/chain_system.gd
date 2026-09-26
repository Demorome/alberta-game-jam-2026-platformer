extends Node2D

@export var ball: Node2D
@export var link_texture: Texture2D  # Drop your joint sprite here
@export var link_length: float = 16.0 # The pixel height/length of one link sprite

@onready var link_container = $LinkContainer

func _process(_delta: float) -> void:
	update_chain()

func update_chain() -> void:
	# Clear previous frames links
	for child in link_container.get_children():
		child.queue_free()
		
	var start_pos = global_position
	var end_pos = ball.global_position
	
	var total_distance = start_pos.distance_to(end_pos)
	var direction = (end_pos - start_pos).normalized()
	
	# Calculate how many links fit in this distance
	var needed_links = int(total_distance / link_length)
	
	# If the distance is small, don't draw anything
	if needed_links == 0:
		return
		
	# Calculate dynamic spacing to perfectly fill the gap (stretching effect)
	var dynamic_spacing = total_distance / needed_links

	for i in range(needed_links):
		var sprite = Sprite2D.new()
		sprite.texture = link_texture
		link_container.add_child(sprite)
		
		# Position each joint along the line
		sprite.global_position = start_pos + direction * (i * dynamic_spacing + (dynamic_spacing / 2))
		
		# Rotate the sprite to face the ball
		sprite.rotation = direction.angle() + PI/2 # Adjust rotation offset if needed
