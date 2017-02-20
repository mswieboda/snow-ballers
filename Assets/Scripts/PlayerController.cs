using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	public GameObject snowballPrefab;
	public float forwardSpeed;
	public float strafeSpeed;
	public float mouseSensitivity;
	public float throwForce;
	public float throwAngleDefault;
	public float releasePointRatioX;
	public float releasePointRatioY;

	private float forwardDirection;
	private float strafeDirection;

	void Start () {
		Cursor.visible = false;
	}

	void Update () {
		movement ();

		if (Input.GetButtonUp ("Fire1")) {
			throwSnowball ();
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

		// -1/1 or 0 depending on if moving
		strafeDirection = Mathf.Abs (horizontal) > 0 ? horizontal / Mathf.Abs (horizontal) : 0;

		// Forward/backward movement
		vertical = Input.GetAxis ("Vertical") * forwardSpeed * Time.deltaTime;
		transform.Translate(0, 0, vertical);

		// -1/1 or 0 depending on if moving
		forwardDirection = Mathf.Abs (vertical) > 0 ? vertical / Mathf.Abs (vertical) : 0;
	}

	private void throwSnowball() {
		Transform arm;
		float forwardForce, playerXAngleDirection;
		float throwAngle = throwAngleDefault;
		Vector3 position, force;
		Quaternion rotation, snowballRotation;
		GameObject snowball;
		Rigidbody rb;

		arm = transform.GetChild(0);

		// Get arm position, add scale.y / 2 to get to hand position
		position = new Vector3(arm.position.x, arm.position.y + transform.localScale.y / 2, arm.position.z);

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
}
