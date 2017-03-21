using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GameMode {
	GameModeManager gameModeManager { get; }
	GameObject gameObject { get; }
	bool inProgress { get; }

	void startGameMode();
	void stopGameMode();
	void displayScoreboard();
}
