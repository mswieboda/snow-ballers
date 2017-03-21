using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameModeManager : NetworkBehaviour {
	public static GameModeManager singleton;

	public int playersInGame = 2;

	private GameMode defaultGameMode;
	private GameMode currentGameMode;
	private MainMenuScript menuScript;

	protected int playersSceneLoaded = 0;

	void Awake() {
		singleton = this;

		defaultGameMode = transform.GetComponentInChildren<GameMode>();
		currentGameMode = defaultGameMode;
		currentGameMode.startGameMode();
	}

	public bool gameInProgress { get { return currentGameMode.inProgress; } }

	[Client]
	public void stopGame() {
		currentGameMode.stopGameMode();
	}

	[Client]
	public void startGame() {
		if (gameInProgress) {
			stopGame();
		}

		Debug.Log("GameModeManager startGame()");
		Debug.Log("GameModeManager startGame() isLocalPlayer: " + isLocalPlayer + " isServer: " + isServer + " isClient: " + isClient);

		// TODO: Temporary, will come from lobby when implemented
		switchGameMode("CaptureTheFlag");
		currentGameMode.startGameMode();
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
		// TODO: Temporary, will come from lobby when implemented
		//       This will be later customized to be the size of the lobby, when all players are ready
		//       and have loaded the scene, then the game starts for all at the same time.
		//       For now this is hardcoded as lobby isn't implemented yet.
		int numPlayers = playersInGame;

		NetworkServer.Spawn(this.gameObject);
		playersSceneLoaded++;
		Debug.Log("GameModeManager OnPlayerSceneLoaded() numPlayers: " + numPlayers + " playersSceneLoaded: " + playersSceneLoaded);
		if (playersSceneLoaded >= numPlayers) {
			Debug.Log("GameModeManager OnPlayerSceneLoaded() all players ready!");
			Debug.Log("GameModeManager OnPlayerSceneLoaded() isLocalPlayer: " + isLocalPlayer + " isServer: " + isServer + " isClient: " + isClient);

			RpcStartGame();
		}
	}
}
