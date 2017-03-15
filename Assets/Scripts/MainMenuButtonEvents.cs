using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototype.NetworkLobby
{
	public class MainMenuButtonEvents : MonoBehaviour {

		public void LoadByIndex(int sceneIndex) {
			SceneManager.LoadScene(sceneIndex);
		}

		public void LoadLobbyManager() {
			LobbyManager lobby = GameObject.FindObjectOfType<LobbyManager>();

			if (lobby != null) {
				Canvas canvas = lobby.gameObject.GetComponent<Canvas>();
				canvas.enabled = true;
				lobby.ShowLobbyManager();
			}
		}
	}
}