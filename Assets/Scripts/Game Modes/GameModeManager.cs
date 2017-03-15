using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameModeManager : NetworkBehaviour {
	private GameMode defaultGameMode;
	public Canvas canvasMenu;

	private GameMode currentGameMode;
	private MainMenuScript menuScript;

	void Awake() {
		defaultGameMode = transform.GetComponentInChildren<GameMode>();
		currentGameMode = defaultGameMode;

		currentGameMode.StartGameMode();
	}

	void Update() {
		if (currentGameMode.isDone) {
			CmdSwitchGameMode(defaultGameMode.gameObject.name);

			// Reset player colors
			NetworkedPlayer [] players = GameObject.FindObjectsOfType<NetworkedPlayer>();

			foreach (NetworkedPlayer player in players) {
				if (player.isLocalPlayer) {
					player.OnStartLocalPlayer();
				}
			}
		}

		// Note: only works on HOST for now because of Player/Client Authority
		if (!currentGameMode.inProgress && Input.GetKeyDown(KeyCode.F1)) {
			showMenu();
		}
	}

	public bool gameInProgress() {
		return currentGameMode == defaultGameMode || (currentGameMode.inProgress || currentGameMode.isDone);
	}

	public void showMenu() {
		canvasMenu.enabled = true;
		canvasMenu.GetComponent<MainMenuScript>().enableCursor();
	}

	public void hideMenu() {
		canvasMenu.enabled = false;
		canvasMenu.GetComponent<MainMenuScript>().disableCursor();
	}

	[Command]
	public void CmdSwitchGameMode(string gameModeName) {
		RpcSwitchGameMode(gameModeName);
	}

	[ClientRpc]
	public void RpcSwitchGameMode(string gameModeName) {
		GameMode gameMode = null;
		Transform gameModeTransform = transform.FindChild(gameModeName);


		if (gameModeTransform != null) {
			gameMode = gameModeTransform.GetComponent<GameMode>();
		}

		if (gameMode != null) {
			currentGameMode.gameObject.SetActive(false);

			currentGameMode = gameMode;
		}
		else {
			currentGameMode = defaultGameMode;
		}

		hideMenu();

		currentGameMode.gameObject.SetActive(true);
		currentGameMode.StartGameMode();
	}
}
