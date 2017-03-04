using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Demo : MonoBehaviour, GameMode {
	public bool inProgress { get; set; }
	public bool isDone { get; set; }

	public void StartGameMode() {
		isDone = false;
		inProgress = false;
	}

	public void displayScoreboard() {
	}
}
