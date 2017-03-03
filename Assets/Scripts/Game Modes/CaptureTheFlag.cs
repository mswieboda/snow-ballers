using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CaptureTheFlag : GameMode {
	private Team [] teams;
	private Text scoreText;

	public override void StartGameMode () {
		teams = transform.GetComponentsInChildren<Team>();
		List<Player> players = new List<Player>();

		players.AddRange(GameObject.FindObjectsOfType<NetworkedPlayerController>());
		players.AddRange(GameObject.FindObjectsOfType<EnemyController>());

		// Split teams into sizes, with remainers in last team
		int [] teamSizes = new int[teams.Length];
		int playersRemaining = players.Count;
		for (int i = 0; i < teams.Length; i++) {
			int size = Mathf.RoundToInt(players.Count / teams.Length);

			if (i >= teams.Length - 1) {
				size = playersRemaining;
			}


			teamSizes[i] = size;
			playersRemaining -= size;
		}

		// shuffle player order
		Utils.Shuffle<Player>(players);

		// Add flags
		foreach (Team team in teams) {
			Transform flagBase = team.transform.FindChild("Flag Base");
			FlagTrigger flagTrigger = flagBase.GetComponentInChildren<FlagTrigger>();
			flagTrigger.team = team;

			Flag flag = team.gameObject.GetComponentInChildren<Flag>();
			flag.team = team;
			flag.setBasePosition(flag.transform.position);
		}

		// assign player teams
		int startIndex = 0;
		for (int t = 0; t < teams.Length; t++) {
			Team team = teams[t];
			int teamSize = teamSizes[t];

			for (int i = startIndex; i < startIndex + teamSize; i++) {
				Player player = players[i];
				team.addPlayer(player);
			}
			startIndex += teamSize;
		}

		foreach (Team team in teams) {
			// respawn teams
			team.spawnPlayers();
		}

		// start game timer
		Debug.Log("GO GO GO!");
	}

	public override void displayScoreboard() {
		foreach (Team team in teams) {
			Canvas scoreboard = team.GetComponentInChildren<Canvas>();
			RectTransform panel = scoreboard.GetComponentInChildren<RectTransform>();
			Text text = panel.GetComponentInChildren<Text>();

			text.text = team.getScore().ToString();
		}
	}
}

public static class Utils {
	public static void Shuffle<T>(this IList<T> list)  
	{  
		int n = list.Count;
		while (n > 1) {
			n--;
			int k = Random.Range(0, n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}  
	}
}