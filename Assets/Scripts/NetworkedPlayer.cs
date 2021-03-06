﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkedPlayer : NetworkBehaviour, Player {
	public Camera mainCamera;

	public GameObject standGO;
	public GameObject standArmGO;
	public GameObject standHeldSnowballGO;
	public Camera standOTSCamera;

	public GameObject crouchGO;
	public GameObject crouchArmGO;
	public GameObject crouchHeldSnowballGO;
	public Camera crouchOTSCamera;

	public GameObject sprintGO;
	public GameObject sprintArmGO;
	public GameObject sprintHeldSnowballGO;
	public Camera sprintOTSCamera;

	public GameObject getSnowballsGO;
	public GameObject getSnowballsArmGO;

	public GameObject snowballPrefab;
	public GameObject snowballIconPrefab;

	private GameObject hudGO;
	private GameObject reticleGO;
	private GameObject snowballPanelGO;
	private GameObject staminaPanelGO;

	public GameObject shovelGO;

	public float normalForwardSpeed = 10;
	public float crouchForwardSpeed = 5;
	public float sprintForwardSpeed = 15;

	public float normalBackwardSpeed = 7;
	public float crouchBackwardSpeed = 2;

	public float normalStrafeSpeed = 15;
	public float crouchStrafeSpeed = 2;

	public float jumpSpeed = 5;
	public float airDragSpeed = -5f;

	public float mouseSensitivity = 5;

	public float throwSnowballForce = 35;
	public float throwFlagForce = 5;
	public float throwAngleDefault = 3;

	public float secondsToThrowSnowball = 0.3f;
	public float secondsToMakeSnowball = 0.5f;

	public int maxSnowballs = 5;

	public bool enableStamina = true;
	public float maxStamina = 100;
	public float minStamina = 5;
	public float walkStaminaAmount = 1;
	public float sprintStaminaAmount = 15;
	public float makeSnowballStaminaAmount = 10;
	public float jumpStaminaAmount = 20;
	public float throwStaminaAmount = 10;
	public float gainStaminaAmount = 1;

	private CharacterController characterController;
	private Vector3 movementVector;
	private float verticalVelocity = 0;

	private float stamina;
	private int snowballs = 5;
	private float timeLastThrownSnowball;
	private float timeGettingSnowballs;

	private bool isAiming = false;
	private bool isSprinting = false;

	private bool isCrouched = false;
	private bool isGettingSnowballs = false;

	private int forwardDirection;
	private float forwardBackward;
	private float sideToSide;
	private int sideToSideDirection;
	private Vector3 jumpVector;
	private Quaternion jumpRotation;
	private float stayGroundedVelocity = -0.1f;
	//hasShovel may be temporary. May switch to array or List<>()
	private bool hasShovel = false;

	private int hitPoints;
	public int maxHitPoints = 3;

	private Snowman snowmanToEnter = null;

	void Update() {
		if (!isLocalPlayer) {
			return;
		}

		showCamera();

		// Note: For now host can switch game mode regardless of game progress
		startGameMode();

		if (!GameModeManager.singleton.gameInProgress) {
			return;
		}

		movement();

		throwActions();

		getSnowballs();

		useItem();

		crouch();

		gainStamina();
	}

	private void setupGOs() {
		hudGO = GameObject.Find("Canvas");
		reticleGO = hudGO.transform.Find("Reticle").gameObject;
		snowballPanelGO = hudGO.transform.Find("Snowball Panel").gameObject;
		staminaPanelGO = hudGO.transform.Find("Stamina Panel Container/Stamina Panel").gameObject;
	}

	/*****************************
	 * Networking
	 *****************************/
	public void spawnInitialization() {
		gameObject.SetActive(true);

		movementVector = Vector3.zero;

		stamina = maxStamina;
		hitPoints = maxHitPoints;

		snowballs = 5;
		isAiming = false;
		isSprinting = false;
		isCrouched = false;
		isGettingSnowballs = false;
		hasShovel = false;

		standGO.SetActive(true);
		standHeldSnowballGO.SetActive(false);
		standOTSCamera.enabled = false;

		crouchGO.SetActive(false);
		crouchHeldSnowballGO.SetActive(false);
		crouchOTSCamera.enabled = false;

		sprintGO.SetActive(false);
		sprintHeldSnowballGO.SetActive(false);
		sprintOTSCamera.enabled = false;

		getSnowballsGO.SetActive(false);

		shovelGO.SetActive(false);

		if (isLocalPlayer) {
			displaySnowballs();
		}
	}

	public override void OnStartLocalPlayer() {
		Debug.Log("NetworkedPlayer OnStartLocalPlayer() isLocalPlayer: " + isLocalPlayer + " isServer: " + isServer + " isClient: " + isClient);
		setupGOs();

		characterController = GetComponent<CharacterController>();

		// Note: Can be overriden by game modes
		changeColor(Color.green);

		// Enable cameras
		mainCamera.GetComponent<AudioListener>().enabled = true;

		Camera[] cameras = new Camera[] {
			mainCamera,
			crouchOTSCamera,
			sprintOTSCamera
		};
		foreach(Camera camera in cameras) {
			camera.enabled = true;
		}

		spawnInitialization();

		CmdOnPlayerSceneLoaded();
	}

	[Command]
	public void CmdOnPlayerSceneLoaded() {
		Debug.Log("NetworkedPlayer CmdOnPlayerSceneLoaded() isLocalPlayer: " + isLocalPlayer + " isServer: " + isServer + " isClient: " + isClient);
		GameModeManager.singleton.OnPlayerSceneLoaded();
	}

	private void startGameMode() {
		if (isServer && isClient && Input.GetButtonDown("Menu")) {
			GameModeManager.singleton.startGame();
		}
	}

	/*****************************
	 * Movement
     *****************************/
	private void movement() {
		mouseLook();
		verticalMovement();

		if (!isGettingSnowballs && characterController.isGrounded) {
			sprint();

			if (hasStamina()) {
				forwardBackwardMovement();
				strafe();
			}
		}

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
			jumpVector.x = sideToSide;

			// Apply air drag
			forwardBackward += forwardBackward * airDragSpeed * Time.deltaTime;
			sideToSide += sideToSideDirection * airDragSpeed * Time.deltaTime;
		}
		else if(characterController.isGrounded && hasStamina() && Input.GetButtonDown("Jump")) {
			jumpRotation = transform.rotation;
			verticalVelocity = jumpSpeed;
			movementVector.y = verticalVelocity;
			useStamina(jumpStaminaAmount / Time.deltaTime);
		}
		else {
			jumpRotation = transform.rotation;
			verticalVelocity = 0;
			jumpVector.y = stayGroundedVelocity;
			movementVector.y = stayGroundedVelocity;
		}

	}

	private void mouseLook() {
		float mouseRotationX = 0;
		float mouseRotationY = Input.GetAxis("Mouse X") * mouseSensitivity;

		if(isAiming) {
			mouseRotationX = Input.GetAxis("Mouse Y") * mouseSensitivity;
		}
		else {
			transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
		}

		transform.Rotate(-mouseRotationX, mouseRotationY, 0);
	}

	private void strafe() {
		sideToSide = Input.GetAxis("Horizontal") * strafeSpeed();

		// -1/1 or 0 depending on if moving
		sideToSideDirection = Mathf.Abs(sideToSide) > 0 ? (int)(sideToSide / Mathf.Abs(sideToSide)) : 0;

		if(Mathf.Abs(sideToSideDirection) > 0) {
			useStamina(walkStaminaAmount);
		}

		movementVector.x = sideToSide;
	}

	private void sprint() {
		bool wasSprinting = isSprinting;

		if(!isCrouched && Input.GetButton("Sprint") && !hasFlag()) {
			isSprinting = true;

			// Are we moving foward?
			if(Input.GetAxisRaw("Vertical") > 0) {
				useStamina(sprintStaminaAmount);
			}
		}
		else {
			isSprinting = false;
		}

		if (isSprinting != wasSprinting) {
			CmdToggleSprint(isSprinting, isCrouched);
		}
	}

	[Command]
	void CmdToggleSprint(bool isSprinting, bool isCrouched) {
		RpcToggleSprint(isSprinting, isCrouched);
	}

	[ClientRpc]
	void RpcToggleSprint(bool isSprinting, bool isCrouched) {
		if (isSprinting) {
			sprintGO.SetActive(true);
			standGO.SetActive(false);

			// Keep snowball held
			if (standHeldSnowballGO.activeSelf) {
				standHeldSnowballGO.SetActive(false);
				sprintHeldSnowballGO.SetActive(true);
			}
		}
		else {
			// Keep snowball held
			if (sprintHeldSnowballGO.activeSelf) {
				sprintHeldSnowballGO.SetActive(false);
				standHeldSnowballGO.SetActive(true);
			}

			if(isCrouched) {
				crouchGO.SetActive(true);
			}
			else {
				standGO.SetActive(true);
			}

			sprintGO.SetActive(false);
		}
	}

	private void forwardBackwardMovement() {
		forwardBackward = Input.GetAxis("Vertical");
		forwardBackward *= forwardBackward >= 0 ? forwardSpeed() : backwardSpeed();

		// -1/1 or 0 depending on if moving
		forwardDirection = Mathf.Abs(forwardBackward) > 0 ? (int)(forwardBackward / Mathf.Abs(forwardBackward)) : 0;

		if(Mathf.Abs(forwardDirection) > 0) {
			useStamina(walkStaminaAmount);
		}

		movementVector.z = forwardBackward;
	}

	private float forwardSpeed() {
		if(isCrouched) {
			return crouchForwardSpeed;
		}
		else if (isSprinting) {
			return sprintForwardSpeed;
		}
		else {
			return normalForwardSpeed;
		}
	}

	private float backwardSpeed() {
		return isCrouched ? crouchBackwardSpeed : normalBackwardSpeed;
	}

	private float strafeSpeed() {
		return isCrouched ? crouchStrafeSpeed : normalStrafeSpeed;
	}

	/*****************************
	 * Stamina
     *****************************/
	private bool hasStamina() {
		if (!enableStamina) {
			return true;
		}

		return stamina > minStamina;
	}

	private void gainStamina() {
		if (!enableStamina) {
			return;
		}

		bool isNotMoving = Mathf.Approximately(Input.GetAxisRaw("Horizontal"), 0) && Mathf.Approximately(Input.GetAxisRaw("Vertical"), 0);
		if (stamina < maxStamina && characterController.isGrounded && isNotMoving && !isGettingSnowballs) {
			stamina += gainStaminaAmount * Time.deltaTime;
			displayStamina();
		}
	}

	private void useStamina(float amount) {
		if (!enableStamina) {
			return;
		}

		stamina -= amount * Time.deltaTime;

		if(stamina < 0) {
			stamina = 0;
		}

		displayStamina();
	}

	private void displayStamina() {
		if (!enableStamina) {
			return;
		}

		RectTransform rt = staminaPanelGO.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(stamina * 3, rt.sizeDelta.y);
		rt.anchoredPosition = new Vector2(stamina * 3 / -2, 0);
	}

	/*****************************
	 * Throw Snowballs
     *****************************/
	private void throwActions() {
		if(snowballs > 0 && !isGettingSnowballs && hasStamina() && Time.time - timeLastThrownSnowball > secondsToThrowSnowball && !hasFlag()) {
			if(Input.GetButtonDown("Throw")) {
				holdSnowball();
			}

			if(Input.GetButtonUp("Throw")) {
				throwSnowball();
			}	
		}
	}

	private GameObject determineHeldSnowballGO() {
		if(isCrouched) {
			return crouchHeldSnowballGO;
		}
		else if(isSprinting) {
			return sprintHeldSnowballGO;
		}
		else {
			return standHeldSnowballGO;
		}
	}

	private void holdSnowball() {
		GameObject heldSnowball = determineHeldSnowballGO();
		heldSnowball.SetActive(true);
	}

	void throwSnowball() {
		GameObject heldSnowball = determineHeldSnowballGO();

		// If we're not holding the "fake" held snowball, don't (create) throw one
		if (heldSnowball.activeSelf == false) {
			return;
		}

		heldSnowball.SetActive(false);

		timeLastThrownSnowball = Time.time;

		removeSnowball();

		useStamina(throwStaminaAmount / Time.deltaTime);

		float forwardForce;
		float throwAngle = throwAngleDefault;
		Vector3 position, force;
		Quaternion snowballRotation;

		// Get arm position, add scale.y / 2 to get to hand position
		position = new Vector3(heldSnowball.transform.position.x, heldSnowball.transform.position.y, heldSnowball.transform.position.z);

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
		forwardForce = forwardDirection * forwardSpeed() + throwSnowballForce;
		force = snowballRotation * new Vector3(0, 0, forwardForce);

		CmdThrowSnowball(position, force);
	}

	[Command]
	void CmdThrowSnowball(Vector3 position, Vector3 force) {
		GameObject snowball;
		Rigidbody rb;

		// Create a new snow ball
		snowball = (GameObject) Instantiate(snowballPrefab, position, Quaternion.identity);

		rb = snowball.GetComponent<Rigidbody>();

		// Apply force to snow ball
		rb.velocity = force;

		// Add it to the network
		NetworkServer.Spawn(snowball);
	}

	/*****************************
	 * Get Snowballs
     *****************************/
	private void getSnowballs() {
		if(characterController.isGrounded && snowballs < maxSnowballs && hasStamina() && Input.GetButton("Reload")) {
			makeSnowballs();
		}
		else {
			stopMakingSnowballs();
		}
	}

	private void makeSnowballs() {
		if (snowballs >= maxSnowballs) {
			return;
		}

		bool wasGettingSnowballs = isGettingSnowballs;

		if (!isGettingSnowballs) {
			timeGettingSnowballs = Time.time;
		}

		if (Time.time - timeGettingSnowballs > secondsToMakeSnowball) {
			timeGettingSnowballs = Time.time;
			addSnowball();
		}

		useStamina(makeSnowballStaminaAmount);

		isGettingSnowballs = true;

		if (wasGettingSnowballs != isGettingSnowballs) {
			CmdToggleGettingSnowballs(isGettingSnowballs, isCrouched, isSprinting);
		}
	}

	private void stopMakingSnowballs() {
		if(!isGettingSnowballs) {
			return;
		}

		timeGettingSnowballs = 0;

		isGettingSnowballs = false;

		CmdToggleGettingSnowballs(isGettingSnowballs, isCrouched, isSprinting);
	}

	[Command]
	void CmdToggleGettingSnowballs(bool isGettingSnowballs, bool isCrouched, bool isSprinting) {
		RpcToggleGettingSnowballs(isGettingSnowballs, isCrouched, isSprinting);
	}

	[ClientRpc]
	void RpcToggleGettingSnowballs(bool isGettingSnowballs, bool isCrouched, bool isSprinting) {
		if (isGettingSnowballs) {
			standGO.SetActive(false);
			crouchGO.SetActive(false);
			sprintGO.SetActive(false);
			getSnowballsGO.SetActive(true);
		}
		else {
			if(isCrouched) {
				crouchGO.SetActive(true);
			}
			else {
				if(isSprinting) {
					sprintGO.SetActive(true);
				}
				else {
					standGO.SetActive(true);
				}
			}

			getSnowballsGO.SetActive(false);
		}
	}

	/*****************************
	 * Crouch
     *****************************/
	private void crouch() {
		if(!isGettingSnowballs && Input.GetButtonDown("Crouch")) {
			isCrouched = !isCrouched;

			CmdToggleCrouch(isCrouched, isSprinting);
		}
	}

	[Command]
	void CmdToggleCrouch(bool isCrouched, bool isSprinting) {
		RpcToggleCrouch(isCrouched, isSprinting);
	}

	[ClientRpc]
	void RpcToggleCrouch(bool isCrouched, bool isSprinting) {
		crouchGO.SetActive(isCrouched);

		if (isSprinting) {
			sprintGO.SetActive(!isCrouched);
		}
		else {
			standGO.SetActive(!isCrouched);
		}

		// If we were holding a snowball, keep holding it after toggle
		if (isCrouched) {
			if(standHeldSnowballGO.activeSelf) {
				standHeldSnowballGO.SetActive(false);
				crouchHeldSnowballGO.SetActive(true);
			}
			else if(sprintHeldSnowballGO.activeSelf) {
				sprintHeldSnowballGO.SetActive(false);
				crouchHeldSnowballGO.SetActive(true);
			}
		}
		else if (!isCrouched && crouchHeldSnowballGO.activeSelf) {
			if(isSprinting) {
				crouchHeldSnowballGO.SetActive(false);
				sprintHeldSnowballGO.SetActive(true);
			}
			else {
				crouchHeldSnowballGO.SetActive(false);
				standHeldSnowballGO.SetActive(true);
			}
		}
	}

	/*****************************
	 * Show Camera
     *****************************/
	private void showCamera() {
		if(!isGettingSnowballs && Input.GetButton("Aim")) {
			showOTSCamera();
		}
		else {
			showMainCamera();
		}
	}

	private void showMainCamera() {
		isAiming = false;

		mainCamera.enabled = true;

		standOTSCamera.enabled = false;
		crouchOTSCamera.enabled = false;
		sprintOTSCamera.enabled = false;

		reticleGO.SetActive(false);
	}

	private void showOTSCamera() {
		isAiming = true;

		if(isCrouched) {
			crouchOTSCamera.enabled = true;
		}
		else {
			if(isSprinting) {
				sprintOTSCamera.enabled = true;
			}
			else {
				standOTSCamera.enabled = true;
			}
		}

		reticleGO.SetActive(true);

		mainCamera.enabled = false;
	}

	/*****************************
	 * Snowball Panel
	 * Display Snowballs
	 * Add/Remove Snowballs
     *****************************/
	private int snowballPanelMarginX = 15;
	private int snowballPanelMarginY = 10;
	private int snowballIconMargin = 1;

	private void resizeSnowballPanel() {
		float snowballIconWidth = snowballIconPrefab.GetComponent<RectTransform>().rect.width;
		float snowballIconHeight = snowballIconPrefab.GetComponent<RectTransform>().rect.width;
		RectTransform rt = snowballPanelGO.GetComponent<RectTransform>();
		float width = snowballPanelMarginX * 2 + (maxSnowballs - 1) * snowballIconWidth + (maxSnowballs - 1) * snowballIconMargin;
		float height = snowballPanelMarginY * 2 + snowballIconHeight;
		rt.sizeDelta = new Vector2(width, height);
		rt.anchoredPosition3D = new Vector3(width / 2, -height / 2, 0);
	}

	private void displaySnowballs() {
		// Deletes all snowball icons
		foreach (Transform child in snowballPanelGO.transform) {
			Destroy(child.gameObject);
		}


		// Creates a snowball icon for each snowball
		for (int i = 0; i < snowballs; i++) {
			GameObject icon = (GameObject)Instantiate(snowballIconPrefab);
			icon.transform.SetParent(snowballPanelGO.transform);
			icon.transform.localPosition = new Vector3(0, 0, 0);
			RectTransform rt = icon.GetComponent<RectTransform>();
			rt.anchoredPosition3D = new Vector3(snowballPanelMarginX + i * rt.rect.width + i * snowballIconMargin, 0, 0);
		}
	}

	private void addSnowball() {
		if (snowballs >= maxSnowballs) {
			return;
		}

		snowballs++;
		displaySnowballs();
	}

	private void removeSnowball() {
		if (snowballs <= 0) {
			return;
		}

		snowballs--;
		displaySnowballs();
	}

	/*****************************
	 * Teamable
     *****************************/
	private Team mTeam;
	public Team team { 
		get { return mTeam; }
		set
		{
			mTeam = value;
			changeColor(mTeam.color);
		}
	}

	public void changeColor(Color color) {
		GameObject[] gameObjects = new GameObject[] { 
			standGO, standArmGO,
			crouchGO, crouchArmGO,
			sprintGO, sprintArmGO,
			getSnowballsGO, getSnowballsArmGO
		};

		// Set meshes to green
		foreach(GameObject gameObject in gameObjects) {
			bool isActive = gameObject.activeSelf;
			gameObject.SetActive(true);
			gameObject.GetComponent<MeshRenderer>().material.color = color;
			gameObject.SetActive(isActive);
		}
	}

	/****************************
	 * Spawnable
	 ****************************/
	public void setPosition(Vector3 position) {
		transform.position = position;
	}

	/****************************
	 * Game Modes - Capture The Flag
	 ****************************/
	public Flag heldFlag { get; set; }

	public bool hasFlag() {
		return heldFlag != null;
	}

	public void pickUp(Flag flag) {
		if (flag.team == team) {
			if (!flag.isAtBase() && !flag.isHeld()) {
				flag.returnToBase();
			}
		}
		else {
			heldFlag = flag;
			flag.holdBy(this, standArmGO.transform.localPosition);
		}
	}

	public void hitBySnowball() {
		CmdHitBySnowball();
	}

	[Command]
	public void CmdHitBySnowball() {
		RpcHitBySnowball();
	}

	[ClientRpc]
	public void RpcHitBySnowball() {
		hitPoints--;

		Debug.Log("RpcHitBySnowball() hitPoints: " + hitPoints);

		if (hitPoints <= 0) {
			if (hasFlag()) {
				Debug.Log("RpcHitBySnowball() dropFlag");
				Flag flag = heldFlag;
				
				if (flag != null) {
					Vector3 flagPosition = flag.transform.position;

					respawn();

					flag.dropFromHolder();
					flag.transform.position = flagPosition;

					return;
				}
			}
			
			Debug.Log("RpcHitBySnowball() respawn");
			respawn();
		}
	}

	private void disable() {
		gameObject.SetActive(false);
	}

	private void respawn() {
		disable();
		GameModeManager.singleton.respawnPlayer(this, isLocalPlayer);
	}

	private void throwFlag() {
		Flag flag = heldFlag;
		Vector3 flagPosition = flag.transform.position;

		float forwardForce;
		float throwAngle = throwAngleDefault;
		Vector3 force;
		Quaternion rotation;

		// Get rotation from Player
		rotation = new Quaternion(
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
		rotation *= Quaternion.Euler(-throwAngle, 0, 0);

		// Calculate speed with new rotation, and forward/backward speed
		forwardForce = forwardSpeed() + throwFlagForce;
		forwardForce = Mathf.Clamp(forwardDirection * forwardSpeed() + throwFlagForce, 0, forwardForce);
		force = rotation * new Vector3(0, 0, forwardForce);

		flag.throwFlag(flagPosition, force);
	}

	void OnTriggerEnter(Collider collision) {
		if (collision.gameObject.CompareTag("Pick Up")) {
			collision.gameObject.SetActive(false);
			hasShovel = true;
		}
			
		Flag flag = collision.gameObject.GetComponent<Flag>();
		if (flag != null) {
			pickUp(flag);
		}

		FlagTrigger flagTrigger = collision.gameObject.GetComponent<FlagTrigger>();
		if (flagTrigger != null) {
			flagTrigger.triggeredBy(this);
		}

		Snowman snowman = collision.gameObject.GetComponent<Snowman>();
		if (snowman != null) {
			Debug.Log("Press Action button to enter snowman");
			snowmanToEnter = snowman;
		}
	}

	void OnTriggerExit(Collider collision) {
		Snowman snowman = collision.gameObject.GetComponent<Snowman>();
		if (snowman != null) {
			Debug.Log("Left snowman, cannot enter it");
			snowmanToEnter = null;
		}
	}

	/****************************
	 * Game Modes - Capture The Flag
	 ****************************/
	[Command]
	void CmdEnterSnowman(bool isLocalPlayer) {
		snowmanToEnter.setPlayer(this, connectionToClient, isLocalPlayer);
		RpcEnterSnowman();

	}

	[ClientRpc]
	void RpcEnterSnowman() {
		snowmanToEnter = null;
		disable();
	}

	/*****************************
	 * Shovel / Inventory
     *****************************/

	private void useItem() {
		if (!Input.GetButtonDown("Action")) {
			return;
		}

		//use a shovel.
		if (hasShovel && characterController.isGrounded) {
			shovelGO.SetActive(true);
		}
		else {
			shovelGO.SetActive(false);
		}

		if (hasFlag()) {
			throwFlag();
		}

		if (snowmanToEnter) {
			CmdEnterSnowman(isLocalPlayer);
		}
	}
}
