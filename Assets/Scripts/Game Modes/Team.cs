using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Team : MonoBehaviour {
	public Color color;
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
			NetworkStartPosition startPostion = startPositions[startIndex];

			player.setPosition(startPostion.transform.position);
		}
	}

	public void addScore(int num) {
		score += num;

		gameMode.displayScoreboard();
	}

	public int getScore() {
		return score;
	}
}
