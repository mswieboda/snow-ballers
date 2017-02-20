using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public GameObject snowballPrefab;
	public GameObject snowballPanel;
	public GameObject snowballIconPrefab;

	public float forwardSpeed = 15;
	public float strafeSpeed = 15;
	public float crouchSpeed = 10;
	public float crouchYAmount = 0.025f;

	public float mouseSensitivity = 5;

	public float throwForce = 50;
	public float throwAngleDefault = 3;

	public int maxSnowballs = 5;

	// TODO: Make a snowball arsenal with
	//       different kinds of snowballs (iceballs, etc)
	private int snowballs = 0;

	// These represent the action of crouching/standing
	// not if they are in the crouched or standing position
	private bool isCrouching = false;
	private bool isStanding = true;
	private bool isGettingSnowball = false;

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

		if (Input.GetButtonDown("Fire3")) {
			getSnowball();
		}

		if (Input.GetButtonDown("Fire2")) {
			toggleCrouch();
		}
	}

	private void movement() {
		float mouseRotation, horizontal, vertical;

		// Mouse look direction
		mouseRotation = Input.GetAxis ("Mouse X") * mouseSensitivity;
		transform.Rotate (0, mouseRotation, 0);

		// Straffing left/right
		horizontal = Input.GetAxis ("Horizontal") * strafeSpeed * Time.deltaTime;
		transform.Translate (horizontal, 0, 0);

		// Forward/backward movement
		vertical = Input.GetAxis ("Vertical") * forwardSpeed * Time.deltaTime;
		transform.Translate(0, 0, vertical);

		// -1/1 or 0 depending on if moving
		forwardDirection = Mathf.Abs (vertical) > 0 ? vertical / Mathf.Abs (vertical) : 0;

		crouch();
		standUp();
	}

	private void crouch() {
		if (!isCrouching) {
			return;
		}

		Vector3 crouchPosition = new Vector3(transform.position.x, crouchYAmount, transform.position.z);

		if (transform.position.y - crouchYAmount <= 0.02f) {
			isCrouching = false;
			isCrouched = true;

			if(isGettingSnowball) {
				isStanding = true;
				addSnowball();
			}

			transform.position = crouchPosition;
		}
		else {
			transform.position = Vector3.Lerp(transform.position, crouchPosition, crouchSpeed * Time.deltaTime);
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
			transform.position = Vector3.Lerp(transform.position, standingPosition, crouchSpeed * Time.deltaTime);
		}
	}

	private void throwSnowball() {
		if (snowballs <= 0) {
			return;
		}

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
		forwardForce = forwardDirection * forwardSpeed + throwForce;
		force = snowballRotation * new Vector3 (0, 0, forwardForce);

		// Apply force to snow ball
		rb.AddForce (force, ForceMode.Impulse);
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
