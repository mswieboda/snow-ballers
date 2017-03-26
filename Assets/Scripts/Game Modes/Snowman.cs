using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Snowman : NetworkBehaviour {
	public Camera mainCamera;

	private Player owner;
	private NetworkConnection ownerNetworkConnection;
	private CharacterController characterController;
	private Vector3 movementVector;

	void Awake() {
		characterController = GetComponent<CharacterController>();
		movementVector = Vector3.zero;
	}

	void Start() {
		netDebug();
	}

	public void initialize() {
		NetworkServer.Spawn(gameObject);
	}

	private void netDebug() {
		Debug.Log("Snowman netDebug() hasAuthority: " + hasAuthority + " isLocalPlayer: " + isLocalPlayer + " localPlayerAuthority: " + localPlayerAuthority);
	}

	[Server]
	public void setPlayer(NetworkedPlayer player, NetworkConnection netConn, bool isLocalPlayer) {
		initialize();

		NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
		networkIdentity.AssignClientAuthority(netConn);

		owner = player;
		ownerNetworkConnection = netConn;

		RpcSetPlayer(isLocalPlayer);
	}

	[ClientRpc]
	void RpcSetPlayer(bool isLocalPlayer) {
		Debug.Log("RpcSetPlayer()");

		if (hasAuthority) {
			mainCamera.gameObject.SetActive(true);
		}
	}

	[Command]
	void CmdUnsetPlayer() {
		NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
		networkIdentity.RemoveClientAuthority(ownerNetworkConnection);

		GameModeManager.singleton.respawnPlayer(owner, true); // owner, isLocalPlayer

		owner = null;

		RpcUnsetPlayer();
	}

	[ClientRpc]
	void RpcUnsetPlayer() {
		Debug.Log("RpcSetPlayer()");

		mainCamera.gameObject.SetActive(false);
	}

	void Update() {
		if (!hasAuthority || !isClient) {
			return;
		}

		if (Input.GetButtonDown("Menu")) {
			CmdUnsetPlayer();
		}

		mouseLook();
		movement();

		movementVector = transform.rotation * movementVector * Time.deltaTime;
		characterController.Move(movementVector);
	}

	private void mouseLook() {
		float mouseRotationY = Input.GetAxis("Mouse X");

		transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

		transform.Rotate(0, mouseRotationY, 0);
	}

	private void movement() {
		float forwardBackward = Input.GetAxis("Vertical");
		forwardBackward *= forwardBackward >= 0 ? 10 : 5;

		movementVector.z = forwardBackward;
	}
}
