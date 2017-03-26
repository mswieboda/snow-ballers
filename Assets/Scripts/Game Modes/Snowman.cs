using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowman : MonoBehaviour {
	public Camera mainCamera;

	private Player owner;

	public void setPlayer(Player player) {
		owner = player;
		mainCamera.gameObject.SetActive(true);
	}

	public void unsetPlayer() {
		GameModeManager.singleton.respawnPlayer(owner, true); // owner, isLocalPlayer

		owner = null;
		mainCamera.gameObject.SetActive(false);
	}

	void Update() {
		if (Input.GetButtonDown("Action")) {
			unsetPlayer();
		}
	}

}
