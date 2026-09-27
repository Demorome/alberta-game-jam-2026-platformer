extends CanvasLayer
## Always-on score display.

@onready var score_label: Label = $ScoreLabel


func _ready() -> void:
	GameManager.score_changed.connect(_on_score_changed)
	_on_score_changed(GameManager.score)


func _on_score_changed(score: int) -> void:
	score_label.text = "Score: %d" % score
