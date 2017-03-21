using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Demo : MonoBehaviour, GameMode {
	public GameModeManager _gameModeManager;
	public GameModeManager gameModeManager { get { return _gameModeManager; } }
	public bool inProgress { get { return mInProgress; } }
	private bool mInProgress;

	public void startGameMode() {
		mInProgress = true;
	}

	public void stopGameMode() {
		mInProgress = false;
	}

	public void displayScoreboard() {
	}
}
