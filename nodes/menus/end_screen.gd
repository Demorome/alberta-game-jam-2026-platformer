extends Control
## Game over or complete.

## Big text on top.
@export var title := "GAME OVER"
## Small line below title.
@export var message := "Better luck next time."
## Wait before input allowed.
@export_range(0.0, 5.0, 0.1, "suffix:s") var input_delay := 2.0
## Hint fade in time.
@export_range(0.0, 3.0, 0.1, "suffix:s") var fade_time := 0.8

var _can_continue := false

@onready var title_label: Label = $Center/Box/TitleLabel
@onready var message_label: Label = $Center/Box/MessageLabel
@onready var stats_label: Label = $Center/Box/StatsLabel
@onready var hint_label: Label = $Center/Box/HintLabel


func _ready() -> void:
	# Stop timer and also hide HUD.
	GameManager.stop_timer()
	GameManager.hide_hud()
	title_label.text = title
	message_label.text = message
	stats_label.text = "Final score: %d\nPlay time: %s\nDeaths: %d" % [
		GameManager.score,
		_format_time(GameManager.play_time),
		GameManager.deaths,
	]
	# "Press to Restart" message hidden until delay.
	hint_label.modulate.a = 0.0
	await get_tree().create_timer(input_delay).timeout
	create_tween().tween_property(hint_label, "modulate:a", 1.0, fade_time)
	_can_continue = true


func _unhandled_input(event: InputEvent) -> void:
	if not _can_continue:
		return
	# Any key or button.
	var pressed := event.is_pressed() and not event.is_echo()
	if pressed and (event is InputEventKey or event is InputEventJoypadButton or event is InputEventMouseButton):
		_can_continue = false
		GameManager.go_to_main_menu()


func _format_time(seconds: float) -> String:
	# mm:ss
	var total := int(seconds)
	return "%02d:%02d" % [total / 60, total % 60]
