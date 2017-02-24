using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public GameObject standingBody;
	public GameObject heldSnowballObject;

	public GameObject crouchBody;
	public GameObject crouchHeldSnowballObject;

	public GameObject snowballPrefab;
	public GameObject snowballPanel;
	public GameObject snowballIconPrefab;

	public Camera MainCamera;
	public Camera OTSCamera;
	public Camera crouchOTSCamera;
	public GameObject reticle;

	public float normalForwardSpeed = 10;
	public float normalBackwardSpeed = 7;
	public float crouchForwardSpeed = 5;
	public float crouchBackwardSpeed = 2;
	public float normalStrafeSpeed = 15;
	public float crouchStrafeSpeed = 2;
	public float mouseSensitivity = 5;
	public float throwForce = 50;
	public float throwAngleDefault = 3;
	public float secondsToThrowSnowball = 0.3f;
	public float secondsToMakeSnowball = 0.5f;
	public int maxSnowballs = 5;

	private CharacterController characterController;
	private Vector3 movementVector;
	private float verticalVelocity = 0;

	// TODO: Make a snowball arsenal with
	//       different kinds of snowballs (iceballs, etc)
	private int snowballs = 10;
	private float timeLastThrownSnowball;

	private bool isGettingSnowball = false;
	private float timeGettingSnowballs;

	private bool isCrouched = false;

	private float forwardDirection;
	private float forwardBackward;
	private float sideToSide;
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

		if(snowballs > 0 && !isGettingSnowball && Time.time - timeLastThrownSnowball > secondsToThrowSnowball) {
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
		
		if (!isGettingSnowball && characterController.isGrounded) {
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
		}
		else if(characterController.isGrounded && Input.GetButtonDown("Jump")) {
			jumpVector.x = sideToSide;
			jumpVector.z = forwardBackward;
			jumpRotation = transform.rotation;
			Debug.Log(jumpVector);
			verticalVelocity = 10;
			movementVector.y = verticalVelocity;
		}
		else {
			verticalVelocity = 0;
			jumpVector.y = -0.1f;
			movementVector.y = -0.1f;
		}
	}

	private void mouseLook() {
		float mouseRotation = Input.GetAxis ("Mouse X") * mouseSensitivity;
		transform.Rotate (0, mouseRotation, 0);
	}

	private void strafe() {
		sideToSide = Input.GetAxis ("Horizontal") * strafeSpeed();
		movementVector.x = sideToSide;
	}

	private void forwardBackwardMovement() {
		
		forwardBackward = Input.GetAxis ("Vertical");
		forwardBackward *= forwardBackward >= 0 ? forwardSpeed() : backwardSpeed();

		// -1/1 or 0 depending on if moving
		forwardDirection = Mathf.Abs(forwardBackward) > 0 ? forwardBackward / Mathf.Abs(forwardBackward) : 0;

		movementVector.z = forwardBackward;
	}

	private float forwardSpeed() {
		return isCrouched ? crouchForwardSpeed : normalForwardSpeed;
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
	private void holdSnowball() {
		GameObject heldSnowball = isCrouched ? crouchHeldSnowballObject : heldSnowballObject;
		heldSnowball.SetActive(true);
	}

	private void throwSnowball() {
		GameObject heldSnowball = isCrouched ? crouchHeldSnowballObject : heldSnowballObject;

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
		if(Input.GetButton("Reload")) {
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

		if (timeGettingSnowballs < 0) {
			timeGettingSnowballs = Time.time;
		}

		if (Time.time - timeGettingSnowballs > secondsToMakeSnowball) {
			timeGettingSnowballs = Time.time;
			addSnowball();
		}

		isGettingSnowball = true;
	}

	private void stopMakingSnowballs() {
		timeGettingSnowballs = -1;

		isGettingSnowball = false;
	}

	/*****************************
	 * Crouch
     *****************************/
	private void crouch() {
		if(!isGettingSnowball && Input.GetButtonDown("Crouch")) {
			toggleCrouch();
		}
	}

	private void toggleCrouch() {
		if (isGettingSnowball) {
			return;
		}

		isCrouched = !isCrouched;

		standingBody.SetActive(!isCrouched);
		crouchBody.SetActive(isCrouched);

		// If we were holding a snowball, keep holding it after toggle
		if (isCrouched && heldSnowballObject.activeSelf) {
			heldSnowballObject.SetActive(false);
			crouchHeldSnowballObject.SetActive(true);
		}
		else if (!isCrouched && crouchHeldSnowballObject.activeSelf) {
			crouchHeldSnowballObject.SetActive(false);
			heldSnowballObject.SetActive(true);
		}
	}

	/*****************************
	 * Show Camera
     *****************************/
	private void showCamera() {
		if(!isGettingSnowball && Input.GetButton("Aim")){
			showOTSCamera();
		}
		else {
			showMainCamera();
		}
	}

	private void showMainCamera() {
		MainCamera.enabled = true;

		OTSCamera.enabled = false;
		crouchOTSCamera.enabled = false;
		reticle.SetActive(false);
	}

	private void showOTSCamera() {
		if(isCrouched) {
			crouchOTSCamera.enabled = true;
		}
		else {
			OTSCamera.enabled = true;
		}

		reticle.SetActive(true);

		MainCamera.enabled = false;
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
		RectTransform rt = snowballPanel.GetComponent<RectTransform>();
		float width = snowballPanelMarginX * 2 + (maxSnowballs - 1) * snowballIconWidth + (maxSnowballs - 1) * snowballIconMargin;
		float height = snowballPanelMarginY * 2 + snowballIconHeight;
		rt.sizeDelta = new Vector2(width, height);
		rt.anchoredPosition3D = new Vector3(width / 2, -height / 2, 0);
	}

	private void displaySnowballs() {
		// Deletes all snowball icons
		foreach (Transform child in snowballPanel.transform) {
			Destroy(child.gameObject);
		}


		// Creates a snowball icon for each snowball
		for (int i = 0; i < snowballs; i++) {
			GameObject icon = (GameObject)Instantiate(snowballIconPrefab);
			icon.transform.SetParent(snowballPanel.transform);
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
