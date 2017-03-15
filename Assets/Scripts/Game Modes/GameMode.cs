using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GameMode {
	GameModeManager gameModeManager { get; }
	GameObject gameObject { get; }
	bool inProgress { get; set; }
	bool isDone { get; set; }

	void StartGameMode();
	void displayScoreboard();
}
