using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Demo : MonoBehaviour, GameMode {
	public GameModeManager _gameModeManager;
	public GameModeManager gameModeManager { get { return _gameModeManager; } }
	public bool inProgress { get; set; }
	public bool isDone { get; set; }

	public void startGameMode() {
		isDone = false;
		inProgress = false;
	}

	public void stopGameMode() {
		isDone = true;
		inProgress = false;
	}

	public void displayScoreboard() {
	}
}
