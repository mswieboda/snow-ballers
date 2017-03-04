using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : MonoBehaviour {
	private GameMode defaultGameMode;
	public Canvas canvasMenu;

	private GameMode currentGameMode;
	private MainMenuScript menuScript;

	void Awake () {
		defaultGameMode = transform.GetComponentInChildren<GameMode>();
		currentGameMode = defaultGameMode;
		switchGameMode(defaultGameMode);
	}

	void Update () {
		if (currentGameMode.isDone) {
			switchGameMode(defaultGameMode);

			// Reset player colors
			NetworkedPlayerController [] players = GameObject.FindObjectsOfType<NetworkedPlayerController>();

			foreach (NetworkedPlayerController player in players) {
				player.OnStartLocalPlayer();
			}
		}

		if (!currentGameMode.inProgress && Input.GetKeyDown(KeyCode.F1)) {
			showMenu();
		}
	}

	public void showMenu() {
		canvasMenu.gameObject.SetActive(true);
		canvasMenu.GetComponent<MainMenuScript>().enableCursor();
	}

	public void hideMenu() {
		canvasMenu.gameObject.SetActive(false);
		canvasMenu.GetComponent<MainMenuScript>().disableCursor();
	}

	public void switchGameMode(GameMode gameMode) {
		if (gameMode != null) {
			currentGameMode.gameObject.SetActive(false);

			currentGameMode = gameMode;
		}
		else {
			currentGameMode = defaultGameMode;
		}

		currentGameMode.gameObject.SetActive(true);
		currentGameMode.StartGameMode();
	}
}
