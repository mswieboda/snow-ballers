using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Team : MonoBehaviour {
	public Color color;
	public GameObject deathCamera;

	private NetworkStartPosition[] startPositions;
	private List<Player> players;
	private GameMode gameMode;

	private int score = 0;

	void Awake() {
		players = new List<Player>();
		startPositions = transform.GetComponentsInChildren<NetworkStartPosition>();
		gameMode = transform.GetComponentInParent<GameMode>();
	}

	public void addPlayer(Player player) {
		players.Add(player);
		player.team = this;
	}

	public List<Player> Players {
		get {
			return players;
		}
	}

	public void spawnPlayers() {
		// respawn players (round robin, for now)
		for (int i = 0; i < players.Count; i++) {
			Player player = players[i];
			int startIndex = i % startPositions.Length;
			NetworkStartPosition startPosition = startPositions[startIndex];

			player.transform.rotation = startPosition.transform.rotation;
			player.setPosition(startPosition.transform.position);
			player.spawnInitialization();
		}
	}

	public void respawnPlayer(Player player) {
		int index = Random.Range(0, players.Count - 1);

		NetworkStartPosition startPosition = startPositions[index];

		player.transform.rotation = startPosition.transform.rotation;
		player.setPosition(startPosition.transform.position);
	}

	public void resetScore() {
		score = 0;

		gameMode.displayScoreboard();
	}

	public void addScore(int num) {
		score += num;

		gameMode.displayScoreboard();
	}

	public int getScore() {
		return score;
	}
}
