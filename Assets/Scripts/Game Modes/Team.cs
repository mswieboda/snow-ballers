using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Team : MonoBehaviour {
	public Color color;
	private NetworkStartPosition[] startPositions;
	private List<Player> players;

	void Awake() {
		players = new List<Player>();
		startPositions = transform.GetComponentsInChildren<NetworkStartPosition>();
	}

	public void addPlayer(Player player) {
		players.Add(player);
		player.setTeam(this);
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
}
