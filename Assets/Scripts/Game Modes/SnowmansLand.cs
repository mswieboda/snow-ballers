using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnowmansLand : MonoBehaviour, GameMode {
	public GameObject snowman;

	private NetworkStartPosition[] startPositions;

	public bool inProgress { get { return mInProgress; } }
	private bool mInProgress;

	void Awake() {
		mInProgress = false;
		startPositions = transform.GetComponentsInChildren<NetworkStartPosition>();
	}

	public void startGameMode() {
		mInProgress = true;
	}

	public void stopGameMode() {
		mInProgress = false;
	}

	public void displayScoreboard() {
	}

	public void respawnPlayer(Player player, bool isLocal) {
		int index = Random.Range(0, startPositions.Length);

		NetworkStartPosition startPosition = startPositions[index];

		player.spawnInitialization();

		player.transform.rotation = startPosition.transform.rotation;
		player.setPosition(startPosition.transform.position);
	} 
}
