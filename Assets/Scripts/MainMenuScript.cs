using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour {
	void Start () {
		enableCursor();
	}

	public void ExitGame() {
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	public void enableCursor() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.Confined;
	}

	public void disableCursor() {
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}
}
