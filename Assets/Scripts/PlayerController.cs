﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public GameObject snowballPrefab;
	public GameObject snowballPanel;
	public GameObject snowballIconPrefab;

	public float normalForwardSpeed = 10;
	public float normalBackwardSpeed = 7;
	public float crouchForwardSpeed = 5;
	public float crouchBackwardSpeed = 2;
	public float normalStrafeSpeed = 15;
	public float crouchStrafeSpeed = 2;

	public float secondsToMakeSnowball = 0.5f;
	public bool crouchPerSnowballControl = false;
	public float crouchAnimationSpeed = 10;
	public float crouchYAmount = 0.025f;

	public float mouseSensitivity = 5;

	public float throwForce = 50;
	public float throwAngleDefault = 3;
	public float secondsToThrowSnowball = 0.3f;
	public int maxSnowballs = 5;

	// TODO: Make a snowball arsenal with
	//       different kinds of snowballs (iceballs, etc)
	private int snowballs = 0;
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

		standingY = transform.position.y;

		clearSnowballs();
	}

	void Update() {
		movement();

		if (Input.GetButtonUp("Fire1")) {
			throwSnowball();
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


				isCrouching = false;
				isStanding = true;
			}

			if (Input.GetButtonDown("Fire2")) {
				toggleCrouch();
			}
		}
	}

	private void movement() {
		if (!isGettingSnowball) {
			forwardBackwardMovement();
			strafe();
		}

		mouseLook();

		crouch();
		standUp();
	}

	private void mouseLook() {
		float mouseRotation = Input.GetAxis ("Mouse X") * mouseSensitivity;
		transform.Rotate (0, mouseRotation, 0);
	}

	private void strafe() {
		float horizontal = Input.GetAxis ("Horizontal") * strafeSpeed() * Time.deltaTime;
		transform.Translate (horizontal, 0, 0);
	}

	private void forwardBackwardMovement() {
		float forwardBackward = Input.GetAxis ("Vertical") * Time.deltaTime;

		forwardBackward *= forwardBackward >= 0 ? forwardSpeed() : backwardSpeed();

		// -1/1 or 0 depending on if moving
		forwardDirection = Mathf.Abs(forwardBackward) > 0 ? forwardBackward / Mathf.Abs(forwardBackward) : 0;

		transform.Translate(0, 0, forwardBackward);
	}

	private void crouch() {
		if (!isCrouching) {
			return;
		}

		Vector3 crouchPosition = new Vector3(transform.position.x, crouchYAmount, transform.position.z);

		if (transform.position.y - crouchYAmount <= 0.02f) {
			if(isGettingSnowball) {
				if (crouchPerSnowballControl) {
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
			isGettingSnowball = false;
			isCrouched = false;

			transform.position = standingPosition;
		}
		else {
			transform.position = Vector3.Lerp(transform.position, standingPosition, crouchAnimationSpeed * Time.deltaTime);
		}
	}

	private void throwSnowball() {
		if (snowballs <= 0 || isGettingSnowball || Time.time - timeLastThrownSnowball < secondsToThrowSnowball) {
			return;
		}

		timeLastThrownSnowball = Time.time;
		removeSnowball();

		Transform arm, hand;
		float forwardForce;
		float throwAngle = throwAngleDefault;
		Vector3 position, force;
		Quaternion rotation, snowballRotation;
		GameObject snowball;
		Rigidbody rb;

		arm = transform.GetChild(0);
		hand = arm.GetChild(0);

		// Get arm position, add scale.y / 2 to get to hand position
		position = new Vector3(hand.position.x, hand.position.y, hand.position.z);

		// Get rotation from Player
		rotation = new Quaternion(
			transform.rotation.x, 
			transform.rotation.y, 
			transform.rotation.z, 
			transform.rotation.w
		);

		// Create a new snow ball
		snowball = (GameObject) Instantiate(snowballPrefab, position, rotation);
		rb = snowball.GetComponent<Rigidbody> ();

		// If going backwards, make the throw angle larger then normal
		if (forwardDirection < 0) {
			throwAngle *= 2;
		}

		// Add an upwards (negative) throwing angle
		snowballRotation = snowball.transform.rotation * Quaternion.Euler (-throwAngle, 0, 0);

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
			
		if (isCrouched) {
			isStanding = true;
		}
		else {
			isCrouching = true;
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

	private void clearSnowballs() {
		snowballs = 0;
		displaySnowballs();
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
