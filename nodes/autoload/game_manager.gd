extends Node
## Global game state singleton.

signal score_changed(score: int)
signal deaths_changed(deaths: int)

const START_SCORE := 10000
const GAME_OVER_SCENE := "res://nodes/menus/game_over.tscn"
# Unused: the last level can just target the game over scene in the scene transition "component".
#const GAME_COMPLETE_SCENE := "res://nodes/menus/game_complete.tscn"
const MAIN_MENU_SCENE := "res://nodes/menus/main_menu.tscn"

var score := START_SCORE
var deaths := 0
# Seconds since run start.
var play_time := 0.0
# Off until run starts.
var running := false

# Sets if the player is currently holding the bag.
var isPlayerHoldingBag := false

func _process(delta: float) -> void:
	if running:
		play_time += delta

## Sets whether the player is holding the bag. (boolean)
func set_player_holding_bag(holding: bool) -> void:
	isPlayerHoldingBag = holding

## Returns whether the player should be holding the bag. Returns a boolean.
func is_player_holding_bag() -> bool:
	return isPlayerHoldingBag

func add_score(amount: int) -> void:
	score += amount
	score_changed.emit(score)


func lose_score(amount: int) -> void:
	# Never below zero
	score = maxi(score - amount, 0)
	score_changed.emit(score)
	if score == 0:
		game_over()


func add_death() -> void:
	deaths += 1
	deaths_changed.emit(deaths)


func start_timer() -> void:
	running = true


func stop_timer() -> void:
	running = false


func restart_level() -> void:
	get_tree().reload_current_scene.call_deferred()


func game_over() -> void:
	get_tree().change_scene_to_file.call_deferred(GAME_OVER_SCENE)


#func game_complete() -> void:
	#get_tree().change_scene_to_file.call_deferred(GAME_COMPLETE_SCENE)


func show_hud() -> void:
	Hud.show()


func hide_hud() -> void:
	Hud.hide()


func go_to_main_menu() -> void:
	reset()
	get_tree().change_scene_to_file.call_deferred(MAIN_MENU_SCENE)


func reset() -> void:
	# New run, clear all.
	score = START_SCORE
	deaths = 0
	play_time = 0.0
	running = false
	score_changed.emit(score)
	deaths_changed.emit(deaths)
