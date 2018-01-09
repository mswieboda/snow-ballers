using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Snowman : NetworkBehaviour {
	public Camera mainCamera;

	public float jumpSpeed = 5;
	public float airDragSpeed = -5f;

	private Player owner;
	private NetworkConnection ownerNetworkConnection;
	private CharacterController characterController;
	private Vector3 movementVector;
	private float verticalVelocity = 0;
	private int forwardDirection;
	private float forwardBackward;
	private Vector3 jumpVector;
	private Quaternion jumpRotation;
	private float stayGroundedVelocity = -0.1f;

	void Awake() {
		characterController = GetComponent<CharacterController>();
		movementVector = Vector3.zero;
	}

	void Start() {
		movementVector = Vector3.zero;
		netDebug();
	}

	public void initialize() {
		NetworkServer.Spawn(gameObject);
		characterController.enabled = false;
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

		characterController.enabled = true;

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

		movement();

		movementVector = transform.rotation * movementVector * Time.deltaTime;
		characterController.Move(movementVector);
	}

	private void movement() {
		mouseLook();
		verticalMovement();

		float forwardBackward = Input.GetAxis("Vertical");
		forwardBackward *= forwardBackward >= 0 ? 10 : 5;

		movementVector.z = forwardBackward;

		if (characterController.isGrounded) {
			movementVector = transform.rotation * movementVector * Time.deltaTime;
			characterController.Move(movementVector);
		}
		else {
			jumpVector = jumpRotation * jumpVector * Time.deltaTime;
			characterController.Move(jumpVector);

		}
	}

	private void verticalMovement() {
		if(!characterController.isGrounded) {
			verticalVelocity += Physics.gravity.y * Time.deltaTime;
			jumpVector.y = verticalVelocity;
			jumpVector.z = forwardBackward;
			jumpVector.x = 0;

			// Apply air drag
			forwardBackward += forwardBackward * airDragSpeed * Time.deltaTime;
		}
		else if(characterController.isGrounded && Input.GetButtonDown("Jump")) {
			jumpRotation = transform.rotation;
			verticalVelocity = jumpSpeed;
			movementVector.y = verticalVelocity;
		}
		else {
			jumpRotation = transform.rotation;
			verticalVelocity = 0;
			jumpVector.y = stayGroundedVelocity;
			movementVector.y = stayGroundedVelocity;
		}

	}

	private void mouseLook() {
		float mouseRotationY = Input.GetAxis("Mouse X");

		transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

		transform.Rotate(0, mouseRotationY, 0);
	}
}
