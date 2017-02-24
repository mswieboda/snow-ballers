using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public Camera mainCamera;
	public GameObject reticleGO;

	public GameObject standGO;
	public GameObject standHeldSnowballGO;
	public Camera standOTSCamera;

	public GameObject crouchGO;
	public GameObject crouchHeldSnowballGO;
	public Camera crouchOTSCamera;

	public GameObject sprintGO;
	public GameObject sprintHeldSnowballGO;
	public Camera sprintOTSCamera;

	public GameObject getSnowballsGO;

	public GameObject snowballPrefab;
	public GameObject snowballPanelGO;
	public GameObject snowballIconPrefab;

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

	public float throwForce = 50;
	public float throwAngleDefault = 3;

	public float secondsToThrowSnowball = 0.3f;
	public float secondsToMakeSnowball = 0.5f;

	public int maxSnowballs = 5;

	private CharacterController characterController;
	private Vector3 movementVector;
	private float verticalVelocity = 0;

	private int snowballs = 10;
	private float timeLastThrownSnowball;
	private float timeGettingSnowballs;

	private bool isSprinting = false;
	private bool isCrouched = false;
	private bool isGettingSnowballs = false;

	private float forwardDirection;
	private float forwardBackward;
	private float sideToSide;
	private float sideToSideDirection;
	private Vector3 jumpVector;
	private Quaternion jumpRotation;

	void Start() {
		Cursor.visible = false;

		characterController = GetComponent<CharacterController>();
		movementVector = new Vector3();

		resizeSnowballPanel();
		displaySnowballs();
	}

	void Update() {
		movement();

		if(snowballs > 0 && !isGettingSnowballs && Time.time - timeLastThrownSnowball > secondsToThrowSnowball) {
			if(Input.GetButtonDown("Throw")) {
				holdSnowball();
			}

			if(Input.GetButtonUp("Throw")) {
				throwSnowball();
			}	
		}
			
		getSnowballs();
		crouch();
		showCamera();
	}

	/*****************************
	 * Movement
     *****************************/
	private void movement() {
		mouseLook();
		verticalMovement();
		
		if (!isGettingSnowballs && characterController.isGrounded) {
			forwardBackwardMovement();
			strafe();
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
		else if(characterController.isGrounded && Input.GetButtonDown("Jump")) {
			jumpRotation = transform.rotation;
			verticalVelocity = jumpSpeed;
			movementVector.y = verticalVelocity;
		}
		else {
			jumpRotation = transform.rotation;
			verticalVelocity = 0;
			jumpVector.y = -0.1f;
			movementVector.y = -0.1f;
		}

	}

	private void mouseLook() {
		float mouseRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
		transform.Rotate(0, mouseRotation, 0);
	}

	private void strafe() {
		sideToSide = Input.GetAxis("Horizontal") * strafeSpeed();

		// -1/1 or 0 depending on if moving
		sideToSideDirection = Mathf.Abs(sideToSide) > 0 ? sideToSide / Mathf.Abs(sideToSide) : 0;

		movementVector.x = sideToSide;
	}

	private void forwardBackwardMovement() {
		forwardBackward = Input.GetAxis("Vertical");
		forwardBackward *= forwardBackward >= 0 ? forwardSpeed() : backwardSpeed();

		// -1/1 or 0 depending on if moving
		forwardDirection = Mathf.Abs(forwardBackward) > 0 ? forwardBackward / Mathf.Abs(forwardBackward) : 0;

		movementVector.z = forwardBackward;
	}

	private float forwardSpeed() {
		if(isCrouched) {
			return crouchForwardSpeed;
		}

		if(Input.GetButton("Sprint")) {
			isSprinting = true;

			sprintGO.SetActive(true);
			standGO.SetActive(false);

			// Keep snowball held
			if (standHeldSnowballGO.activeSelf) {
				standHeldSnowballGO.SetActive(false);
				sprintHeldSnowballGO.SetActive(true);
			}

			return sprintForwardSpeed;
		}

		// Keep snowball held
		if (sprintHeldSnowballGO.activeSelf) {
			sprintHeldSnowballGO.SetActive(false);
			standHeldSnowballGO.SetActive(true);
		}

		isSprinting = false;

		standGO.SetActive(true);
		sprintGO.SetActive(false);

		return normalForwardSpeed;
	}

	private float backwardSpeed() {
		return isCrouched ? crouchBackwardSpeed : normalBackwardSpeed;
	}

	private float strafeSpeed() {
		return isCrouched ? crouchStrafeSpeed : normalStrafeSpeed;
	}

	/*****************************
	 * Throw Snowballs
     *****************************/
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

	private void throwSnowball() {
		GameObject heldSnowball = determineHeldSnowballGO();

		// If we're not holding the "fake" held snowball, don't (create) throw one
		if (heldSnowball.activeSelf == false) {
			return;
		}

		heldSnowball.SetActive(false);

		timeLastThrownSnowball = Time.time;
		removeSnowball();


		float forwardForce;
		float throwAngle = throwAngleDefault;
		Vector3 position, force;
		Quaternion snowballRotation;
		GameObject snowball;
		Rigidbody rb;

		// Get arm position, add scale.y / 2 to get to hand position
		position = new Vector3(heldSnowball.transform.position.x, heldSnowball.transform.position.y, heldSnowball.transform.position.z);

		// Create a new snow ball
		snowball = (GameObject) Instantiate(snowballPrefab, position, Quaternion.identity);

		rb = snowball.GetComponent<Rigidbody> ();

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
		snowballRotation *= Quaternion.Euler (-throwAngle, 0, 0);

		// Calculate speed with new rotation, and forward/backward speed
		forwardForce = forwardDirection * forwardSpeed() + throwForce;
		force = snowballRotation * new Vector3 (0, 0, forwardForce);

		// Apply force to snow ball
		rb.AddForce (force, ForceMode.Impulse);
	}

	/*****************************
	 * Get Snowballs
     *****************************/
	private void getSnowballs() {
		if(snowballs < maxSnowballs && Input.GetButton("Reload")) {
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

		if (!isGettingSnowballs) {
			timeGettingSnowballs = Time.time;

			standGO.SetActive(false);
			crouchGO.SetActive(false);
			getSnowballsGO.SetActive(true);
		}

		if (Time.time - timeGettingSnowballs > secondsToMakeSnowball) {
			timeGettingSnowballs = Time.time;
			addSnowball();
		}

		isGettingSnowballs = true;
	}

	private void stopMakingSnowballs() {
		if(!isGettingSnowballs) {
			return;
		}

		timeGettingSnowballs = 0;

		if(isCrouched) {
			crouchGO.SetActive(true);
		}
		else {
			standGO.SetActive(true);
		}

		getSnowballsGO.SetActive(false);

		isGettingSnowballs = false;
	}

	/*****************************
	 * Crouch
     *****************************/
	private void crouch() {
		if(!isGettingSnowballs && Input.GetButtonDown("Crouch")) {
			toggleCrouch();
		}
	}

	private void toggleCrouch() {
		if (isGettingSnowballs) {
			return;
		}

		isCrouched = !isCrouched;

		standGO.SetActive(!isCrouched);
		sprintGO.SetActive(!isCrouched);
		crouchGO.SetActive(isCrouched);

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
		if(!isGettingSnowballs && Input.GetButton("Aim")){
			showOTSCamera();
		}
		else {
			showMainCamera();
		}
	}

	private void showMainCamera() {
		mainCamera.enabled = true;

		standOTSCamera.enabled = false;
		crouchOTSCamera.enabled = false;
		sprintOTSCamera.enabled = false;

		reticleGO.SetActive(false);
	}

	private void showOTSCamera() {
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
}
