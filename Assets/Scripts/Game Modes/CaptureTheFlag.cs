using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CaptureTheFlag : MonoBehaviour, GameMode {
	public GameModeManager _gameModeManager;
	public GameModeManager gameModeManager { get { return _gameModeManager; } }

	public int scoreToWin = 3;
	public int startTimer = 3;
	public float respawnTime = 3f;

	public Canvas canvas;
	public Text infoText;

	private Team [] teams;

	public bool inProgress { get { return mInProgress; } }
	private bool mInProgress;

	void Awake() {
		canvas = GetComponentInChildren<Canvas>();
		infoText = canvas.GetComponentInChildren<Text>();
		canvas.enabled = false;

		teams = transform.GetComponentsInChildren<Team>();

		foreach (Team team in teams) {
			team.deathCamera.SetActive(false);
		}

		mInProgress = false;
	}

	public void startGameMode() {
		List<Player> players = new List<Player>();

		players.AddRange(GameObject.FindObjectsOfType<NetworkedPlayer>());
		players.AddRange(GameObject.FindObjectsOfType<BotPlayer>());

		// Add flags
		foreach (Team team in teams) {
			Flag flag = team.gameObject.GetComponentInChildren<Flag>();
			flag.team = team;
			flag.setBasePosition(flag.transform.position);

			Transform flagBase = team.transform.FindChild("Flag Base");
			FlagTrigger flagTrigger = flagBase.GetComponentInChildren<FlagTrigger>();
			flagTrigger.team = team;
			flagTrigger.teamFlag = flag;
		}

		assignTeams(players);

		foreach (Team team in teams) {
			team.spawnPlayers();
		}

		startGame();
	}

	public void stopGameMode() {
		canvas.enabled = false;
		mInProgress = false;
	}

	private void startGame() {
		canvas.enabled = true;
		StartCoroutine(countdownCoroutine(startTimer));
	}

	private IEnumerator countdownCoroutine(int secondsLeft) {
		if (secondsLeft > 0) {
			infoText.text = secondsLeft.ToString();
		}
		else if (secondsLeft == 0) {
			mInProgress = true;

			infoText.text = "GO GO GO!";
		}
		else {
			canvas.enabled = false;

			yield break;
		}

		yield return new WaitForSeconds(1f);

		StartCoroutine(countdownCoroutine(secondsLeft - 1));
	}

	public void displayScoreboard() {
		foreach (Team team in teams) {
			Canvas scoreboard = team.GetComponentInChildren<Canvas>();
			RectTransform panel = scoreboard.GetComponentInChildren<RectTransform>();
			Text text = panel.GetComponentInChildren<Text>();

			text.text = team.getScore().ToString();
		}

		checkWinCondition();
	}

	public void checkWinCondition() {
		foreach (Team team in teams) {
			if (team.getScore() >= scoreToWin) {
				displayWin(team);
			}
		}
	}

	public void displayWin(Team winningTeam) {
		canvas.enabled = true;
		infoText.text = winningTeam.name + " wins!!!";

		StartCoroutine(endGame(3f));
	}

	private IEnumerator endGame(float waitTime) {
		yield return new WaitForSeconds(waitTime);

		stopGameMode();
	}

	// Assigns teams round robin style
	private void assignTeams(IList<Player> players) {
		Team team;
		int teamIndex = 0;

		for (int i = 0; i < players.Count; i++) {
			Player player = players[i];

			team = teams[teamIndex];
			team.addPlayer(player);

			teamIndex++;

			if (teamIndex >= teams.Length) {
				teamIndex = 0;
			}
		}
	}

	public void respawnPlayer(Player player, bool isLocal)
	{
		if (isLocal) {
			player.team.deathCamera.SetActive(true);
		}

		StartCoroutine(respawnWait(respawnTime, player));
	}

	private IEnumerator respawnWait(float waitTime, Player player) {
		yield return new WaitForSeconds(waitTime);

		player.team.deathCamera.SetActive(false);
		player.spawnInitialization();
		player.team.respawnPlayer(player);
	}
}
