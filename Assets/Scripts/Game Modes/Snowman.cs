using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Snowman : NetworkBehaviour {
	public Camera mainCamera;
	public GameObject standHeldSnowballsGO;
	public GameObject bigSnowballPrefab;

	public float jumpSpeed = 5;
	public float airDragSpeed = -5f;

	public float secondsToThrowSnowball = 0.3f;
	public float secondsToMakeSnowball = 0.5f;

	public float throwSnowballForce = 35;
	public float throwAngleDefault = 3;

	private Player owner;
	private NetworkConnection ownerNetworkConnection;
	private CharacterController characterController;
	private Vector3 movementVector;
	private float verticalVelocity = 0;
	private int forwardDirection = 0;
	private float forwardBackward;
	private Vector3 jumpVector;
	private Quaternion jumpRotation;
	private float stayGroundedVelocity = -0.1f;

	private int snowballs = 3;
	private float timeLastThrownSnowball;
	private float timeGettingSnowballs;

	private bool isGettingSnowballs = false;

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

		throwActions();

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

	/*****************************
	 * Throw Snowballs
     *****************************/
	private void throwActions() {
		if(snowballs > 0 && !isGettingSnowballs && Time.time - timeLastThrownSnowball > secondsToThrowSnowball) {
			if(Input.GetButtonDown("Throw")) {
				holdSnowball();
			}

			if(Input.GetButtonUp("Throw")) {
				GameObject heldSnowball = determineHeldSnowballGO();

				// If we're not holding the "fake" held snowball, don't (create) throw one
				if (heldSnowball.activeSelf == false) {
					return;
				}

				heldSnowball.SetActive(false);

				Transform [] heldSnowballTransforms = determineHeldSnowballGO().GetComponentsInChildren<Transform>();

				foreach (Transform heldSnowballTransform in heldSnowballTransforms) {
					throwSnowball(heldSnowballTransform);
				}
			}	
		}
	}

	private GameObject determineHeldSnowballGO() {
//		if(isCrouched) {
//			return crouchHeldSnowballGO;
//		}
//		else if(isSprinting) {
//			return sprintHeldSnowballGO;
//		}
//		else {
			return standHeldSnowballsGO;
//		}
	}

	private void holdSnowball() {
		GameObject heldSnowball = determineHeldSnowballGO();
		heldSnowball.SetActive(true);
	}

	void throwSnowball(Transform heldSnowballTransform) {
		timeLastThrownSnowball = Time.time;

//		TODO:
//		removeSnowball();

		float forwardForce;
		float throwAngle = throwAngleDefault;
		Vector3 position, force;
		Quaternion snowballRotation;

		// Get arm position, add scale.y / 2 to get to hand position
		position = new Vector3(heldSnowballTransform.position.x, heldSnowballTransform.position.y, heldSnowballTransform.position.z);

		// Get rotation from Player
		snowballRotation = new Quaternion(
			transform.rotation.x, 
			transform.rotation.y, 
			transform.rotation.z, 
			transform.rotation.w
		);

		// If going backwards, make the throw angle larger then normal
		if (forwardDirection < 0) {
			throwAngle *= 2;
		}

		// Add an upwards (negative) throwing angle
		snowballRotation *= Quaternion.Euler(-throwAngle, 0, 0);

		// Calculate speed with new rotation, and forward/backward speed
		forwardForce = forwardDirection /* * forwardSpeed() */ + throwSnowballForce;
		force = snowballRotation * new Vector3(0, 0, forwardForce);

		CmdThrowSnowball(position, force);
	}

	[Command]
	void CmdThrowSnowball(Vector3 position, Vector3 force) {
		GameObject snowball;
		Rigidbody rb;

		// Create a new snow ball
		snowball = (GameObject) Instantiate(bigSnowballPrefab, position, Quaternion.identity);

		rb = snowball.GetComponent<Rigidbody>();

		// Apply force to snow ball
		rb.velocity = force;

		// Add it to the network
		NetworkServer.Spawn(snowball);
	}
}
