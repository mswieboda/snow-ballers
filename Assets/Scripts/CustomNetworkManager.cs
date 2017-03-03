using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager {
	public Camera defaultCamera;

	public override void OnStartClient(NetworkClient client) {
		defaultCamera.gameObject.SetActive(false);
	}
}
