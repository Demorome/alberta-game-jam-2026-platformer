extends Node2D

## Enable automatic setup - finds Player and moneybag in scene
@export var auto_setup := false

## The bagNode that the chain is attached to
@export var bagNode: Node2D

## The texture used for each link in the chain
@export var link_texture: Texture2D

## How much space for each link in the chain
@export var link_length: float = 16.0

@onready var link_container = $LinkContainer


func _ready() -> void:
	if auto_setup:
		_auto_setup_chain.call_deferred()


func _auto_setup_chain() -> void:
	
	# Find moneybag and get its CharacterBody2D child
	var moneybag = get_tree().current_scene.find_child("MoneyBag", true, false)
	if moneybag == null:
		push_error("🔗 [ChainSystem] ❌ Could not find 'moneybag' node")
		return
	
	var moneybag_body = moneybag.find_child("CharacterBody2D", true, false)
	if moneybag_body == null:
		push_error("🔗 [ChainSystem] ❌ Could not find CharacterBody2D child in moneybag")
		return
	
	# Set the bagNode reference
	bagNode = moneybag_body

	# Find Player and get its CharacterBody2D child
	var player = get_tree().current_scene.find_child("Player", true, false)
	if player == null:
		push_error("🔗 [ChainSystem] ❌ Could not find 'Player' node")
		return
	
	var player_body = player.find_child("CharacterBody2D", true, false)
	if player_body == null:
		push_error("🔗 [ChainSystem] ❌ Could not find CharacterBody2D child in Player")
		return
	
	# Reparent this chain to the player's CharacterBody2D so the chain start position follow it
	reparent(player_body)
	self.position.x = 0;
	self.position.y = -16;



func _process(_delta: float) -> void:
	update_chain()


func update_chain() -> void:
	# Safe escape! Skip if bagNode not set
	if bagNode == null:
		return
	
	# Delete all links to start fresh
	for child in link_container.get_children():
		child.queue_free()
		
	var start_position = global_position
	var end_position = bagNode.global_position
	
	var total_distance = start_position.distance_to(end_position)
	var direction = (end_position - start_position).normalized()
	
	var needed_links = int(total_distance / link_length)
	
	# For small distances don't draw anything
	if needed_links == 0:
		return
		
	# Trying to calculate some sort of stretching effect
	# var dynamic_spacing = total_distance / needed_links

	for i in range(needed_links):
		var singleLinkSprite = Sprite2D.new()
		singleLinkSprite.texture = link_texture
		link_container.add_child(singleLinkSprite)
		
		# Position each joint
		singleLinkSprite.global_position = start_position + direction * (link_length * i) #* (i * dynamic_spacing + (dynamic_spacing / 2))
		
		# Rotate the link sprite to face the bagNode
		singleLinkSprite.rotation = direction.angle() + PI/2
