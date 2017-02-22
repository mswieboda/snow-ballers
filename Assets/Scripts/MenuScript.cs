using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour {

	//CursorLockMode mode;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Update is called once per frame
	void Update () {
		//TODO: THIS IS TEMPORARY AND SHOULD BE MOVED TO A BUTTON
		if (Input.GetKey("escape")) {
			Application.Quit();
		}
	}
}
