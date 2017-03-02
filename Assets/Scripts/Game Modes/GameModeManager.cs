using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : MonoBehaviour {
	public GameMode defaultGameMode;
	public GameMode captureTheFlag;
	public Canvas canvasMenu;

	private GameMode currentGameMode;
	private MainMenuScript menuScript;

	// Use this for initialization
	void Start () {
		switchGameMode(null);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.F1)) {
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
