using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update () {
		if (Input.GetButtonDown("Menu")) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Locked;
			SceneManager.LoadScene(0);
		}
	}
}
