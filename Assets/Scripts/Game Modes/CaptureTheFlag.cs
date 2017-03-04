using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CaptureTheFlag : MonoBehaviour, GameMode {
	public int scoreToWin = 3;

	private Team [] teams;
	private Text scoreText;

	public bool inProgress { get; set; }
	public bool isDone { get; set; }

	public void StartGameMode () {
		isDone = false;
		inProgress = true;

		teams = transform.GetComponentsInChildren<Team>();

		List<Player> players = new List<Player>();

		players.AddRange(GameObject.FindObjectsOfType<NetworkedPlayerController>());
		players.AddRange(GameObject.FindObjectsOfType<EnemyController>());

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

		// start game timer
		Debug.Log("GO GO GO!");
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

	public void displayWin(Team team) {
		inProgress = false;

		Debug.Log(team.name + " wins!!!");

		StartCoroutine(endGame(3f));
	}

	// every 2 seconds perform the print()
	private IEnumerator endGame(float waitTime) {
		yield return new WaitForSeconds(waitTime);

		isDone = true;
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
}
