using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameModeManager : NetworkBehaviour {
	public static GameModeManager singleton;

	private GameMode defaultGameMode;
	public Canvas canvasMenu;

	private GameMode currentGameMode;
	private MainMenuScript menuScript;

	protected int playersSceneLoaded = 0;

	void Awake() {
		singleton = this;

		defaultGameMode = transform.GetComponentInChildren<GameMode>();
		currentGameMode = defaultGameMode;
	}

	public bool gameInProgress() {
		return currentGameMode == defaultGameMode || (currentGameMode.inProgress || currentGameMode.isDone);
	}

	[Client]
	private void startGame() {
		Debug.Log("GameModeManager startGame()");
		Debug.Log("GameModeManager startGame() isLocalPlayer: " + isLocalPlayer + " isServer: " + isServer + " isClient: " + isClient);
		switchGameMode(Prototype.NetworkLobby.LobbyPlayerList._instance.gameModeName);
		currentGameMode.StartGameMode();
	}

	[ClientRpc]
	private void RpcStartGame() {
		Debug.Log("GameModeManager RpcStartGame()");
		startGame();
	}

	[Client]
	void switchGameMode(string gameModeName) {
		Debug.Log("GameModeManager switchGameMode(" + gameModeName + ")");
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
			Debug.Log("Game mode \"" + gameModeName + "\" doesn't exist, loading default game mode");
			currentGameMode = defaultGameMode;
		}

		currentGameMode.gameObject.SetActive(true);
	}

	public void OnPlayerSceneLoaded() {
		int numPlayers = Prototype.NetworkLobby.LobbyManager.s_Singleton.numPlayers;
		NetworkServer.Spawn(this.gameObject);
		playersSceneLoaded++;
		Debug.Log("GameModeManager OnPlayerSceneLoaded() numPlayers: " + numPlayers + " playersSceneLoaded: " + playersSceneLoaded);
		if (playersSceneLoaded >= numPlayers) {
			Debug.Log("GameModeManager OnPlayerSceneLoaded() all players ready!");
			Debug.Log("GameModeManager OnPlayerSceneLoaded() isLocalPlayer: " + isLocalPlayer + " isServer: " + isServer + " isClient: " + isClient);

			if (isServer && isClient) {
				// TODO: shouldn't be necessary, but for some reason
				// when host and client, with no other clients 
				// this class can't call an RPC function!?!
				startGame();
			}
			else {
				RpcStartGame();
			}
		}
	}
}
