using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameModeManager : NetworkBehaviour {
	private GameMode defaultGameMode;
	public Canvas canvasMenu;

	private GameMode currentGameMode;
	private MainMenuScript menuScript;

	void Awake () {
		defaultGameMode = transform.GetComponentInChildren<GameMode>();
		currentGameMode = defaultGameMode;

		if (isServer) {
			RpcSwitchGameMode(defaultGameMode.gameObject.name);
		}
	}

	void Update () {
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
			CmdShowMenu();
		}
	}

	[Command]
	public void CmdShowMenu() {
		RpcShowMenu();
	}

	[ClientRpc]
	public void RpcShowMenu() {
		canvasMenu.gameObject.SetActive(true);
		canvasMenu.GetComponent<MainMenuScript>().enableCursor();
	}

	[Command]
	public void CmdHideMenu() {
		RpcHideMenu();
	}

	[ClientRpc]
	public void RpcHideMenu() {
		hideMenu();
	}

	public void hideMenu() {
		canvasMenu.gameObject.SetActive(false);
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
