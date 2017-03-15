using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {
	void Start () {
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update () {
		if (Input.GetButtonDown("Menu")) {
			Prototype.NetworkLobby.LobbyManager.s_Singleton.ServerChangeScene("Main Menu");
		}
	}
}
