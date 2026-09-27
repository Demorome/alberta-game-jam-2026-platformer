extends Node
## Global game state singleton.

signal score_changed(score: int)
signal deaths_changed(deaths: int)

const START_SCORE := 10000

var score := START_SCORE
var deaths := 0
# Seconds since run start.
var play_time := 0.0


func _process(delta: float) -> void:
	play_time += delta


func add_score(amount: int) -> void:
	score += amount
	score_changed.emit(score)


func lose_score(amount: int) -> void:
	# Never below zero
	score = maxi(score - amount, 0)
	score_changed.emit(score)


func add_death() -> void:
	deaths += 1
	deaths_changed.emit(deaths)


func restart_level() -> void:
	get_tree().reload_current_scene.call_deferred()


func reset() -> void:
	# New run, clear all.
	score = START_SCORE
	deaths = 0
	play_time = 0.0
	score_changed.emit(score)
	deaths_changed.emit(deaths)
