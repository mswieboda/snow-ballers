﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public GameObject armObject;
	public GameObject heldSnowballObject;

	public GameObject crouchObject;
	public GameObject crouchArmObject;
	public GameObject crouchHeldSnowballObject;

	public GameObject snowballPrefab;
	public GameObject snowballPanel;
	public GameObject snowballIconPrefab;
	public Camera MainCamera;
	public Camera OTSCamera;
	public GameObject reticle;

	public float normalForwardSpeed = 10;
	public float normalBackwardSpeed = 7;
	public float crouchForwardSpeed = 5;
	public float crouchBackwardSpeed = 2;
	public float normalStrafeSpeed = 15;
	public float crouchStrafeSpeed = 2;

	public float secondsToMakeSnowball = 0.5f;
	public bool crouchPerSnowballControl = false;
	public float crouchAnimationSpeed = 10;
	public float crouchYTarget = 0.025f;

	public float mouseSensitivity = 5;

	public float throwForce = 50;
	public float throwAngleDefault = 3;
	public float secondsToThrowSnowball = 0.3f;
	public int maxSnowballs = 5;

	private CharacterController characterController;
	private Vector3 movementVector;
	private float verticalVelocity = 0;

	// TODO: Make a snowball arsenal with
	//       different kinds of snowballs (iceballs, etc)
	private int snowballs = 10;
	private float timeLastThrownSnowball;

	// These represent the action of crouching/standing
	// not if they are in the crouched or standing position
	private bool isCrouching = false;
	private bool isStanding = true;
	private bool isGettingSnowball = false;
	private float timeCrouchPressed;

	// Represents if they are crouched (true) or standing (false)
	private bool isCrouched = false;
	private float standingY;

	private float forwardDirection;

	void Start() {
		Cursor.visible = false;

		characterController = GetComponent<CharacterController>();
		movementVector = characterController.transform.position;
		standingY = transform.position.y;

		displaySnowballs();
	}

	void Update() {
		movement();

		if (snowballs > 0 && !isGettingSnowball && Time.time - timeLastThrownSnowball > secondsToThrowSnowball) {
			if (Input.GetButtonDown("Fire1")) {
				holdSnowball();
			}

			if (Input.GetButtonUp("Fire1")) {
				throwSnowball();
			}	
		}

		if (crouchPerSnowballControl) {
			if (Input.GetButtonDown("Fire3")) {
				getSnowball();
			}

			if (Input.GetButtonDown("Fire2")) {
				toggleCrouch();
			}
		}
		else {
			if (Input.GetButtonDown("Fire3")) {
				timeCrouchPressed = Time.time;

				getSnowball();
			}

			if (Input.GetButtonUp("Fire3")) {
				timeCrouchPressed = 0;

				isGettingSnowball = false;
				isCrouching = false;
				isStanding = true;
			}

			// TODO: Disabling for now as it's not compatible with
			//       CharacterController and moving halfway in Plane
			 if (Input.GetButtonDown("Fire4")) {
				toggleCrouch();
			 }

			if(Input.GetButton("Fire2")){
				showOTSCamera();
			}
			else {
				showMainCamera();
			}
		}
			

	}

	private void movement() {
		verticalMovement();

		if (!isGettingSnowball) {
			forwardBackwardMovement();
			strafe();
		}

		mouseLook();

		crouch();
		standUp();

		if(characterController.enabled) {
			// Apply rotation to vector
			movementVector = transform.rotation * movementVector * Time.deltaTime;
			characterController.Move(movementVector);
		}
	}

	private void verticalMovement() {
		if(!characterController.isGrounded) {
			verticalVelocity += Physics.gravity.y * Time.deltaTime;
			movementVector.y = verticalVelocity;
		}
		else if(characterController.isGrounded && Input.GetButtonDown("Jump")) {
			verticalVelocity = 10;
			movementVector.y = verticalVelocity;
		}
		else {
			verticalVelocity = 0;
			movementVector.y = -0.1f;
		}
	}

	private void mouseLook() {
		float mouseRotation = Input.GetAxis ("Mouse X") * mouseSensitivity;
		transform.Rotate (0, mouseRotation, 0);
	}

	private void strafe() {
		movementVector.x = Input.GetAxis ("Horizontal") * strafeSpeed();
	}

	private void forwardBackwardMovement() {
		float forwardBackward = Input.GetAxis ("Vertical");

		forwardBackward *= forwardBackward >= 0 ? forwardSpeed() : backwardSpeed();

		// -1/1 or 0 depending on if moving
		forwardDirection = Mathf.Abs(forwardBackward) > 0 ? forwardBackward / Mathf.Abs(forwardBackward) : 0;

		movementVector.z = forwardBackward;
	}

	private void crouch() {
		if (!isCrouching) {
			return;
		}

		characterController.enabled = false;

		Vector3 crouchPosition = new Vector3(transform.position.x, crouchYTarget, transform.position.z);

		if (transform.position.y - crouchYTarget <= 0.02f) {
			if(isGettingSnowball) {
				if (crouchPerSnowballControl) {
					isGettingSnowball = false;
					isCrouching = false;
					isCrouched = true;
					isStanding = true;

					addSnowball();
				}
				else {
					isCrouched = true;

					if (Time.time - timeCrouchPressed > secondsToMakeSnowball) {
						timeCrouchPressed = Time.time;
						addSnowball();
					}
				}
			}
			else {
				isCrouching = false;
				isCrouched = true;
			}

			transform.position = crouchPosition;
		}
		else {
			transform.position = Vector3.Lerp(transform.position, crouchPosition, crouchAnimationSpeed * Time.deltaTime);
		}
	}

	private void standUp() {
		if (!isStanding) {
			return;
		}

		Vector3 standingPosition = new Vector3(transform.position.x, standingY, transform.position.z);

		if (standingY - transform.position.y <= 0.02f) {
			isStanding = false;
			isCrouched = false;

			transform.position = standingPosition;

			characterController.enabled = true;
		}
		else {
			transform.position = Vector3.Lerp(transform.position, standingPosition, crouchAnimationSpeed * Time.deltaTime);
		}
	}

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

	private float forwardSpeed() {
		return isCrouched ? crouchForwardSpeed : normalForwardSpeed;
	}

	private float backwardSpeed() {
		return isCrouched ? crouchBackwardSpeed : normalBackwardSpeed;
	}

	private float strafeSpeed() {
		return isCrouched ? crouchStrafeSpeed : normalStrafeSpeed;
	}

	private void toggleCrouch() {
		if (isGettingSnowball || isStanding || isCrouching) {
			return;
		}

		isCrouched = !isCrouched;

		MeshRenderer renderer = GetComponent<MeshRenderer>();

		renderer.enabled = !isCrouched;
		armObject.SetActive(!isCrouched);

		crouchObject.SetActive(isCrouched);
		crouchArmObject.SetActive(isCrouched);

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

	private void getSnowball() {
		if (snowballs >= maxSnowballs || isGettingSnowball) {
			return;
		}

		isCrouching = true;
		isGettingSnowball = true;
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
			rt.anchoredPosition3D = new Vector3(10 + i * rt.rect.width + i * 1, 0, 0);
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

	public void showMainCamera() {
		MainCamera.enabled = true;
		OTSCamera.enabled = false;
		reticle.SetActive(false);
	}

	public void showOTSCamera() {
		OTSCamera.enabled = true;
		MainCamera.enabled = false;
		reticle.SetActive(true);
	}
}
