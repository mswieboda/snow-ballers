using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {
	public float strafeSpeed = 1f;
	public float timeToMove = 1.5f;

	private CharacterController characterController;
	private Vector3 movementVector;
	private float timeMoving;
	private int direction = 0;

	void Start() {
		characterController = GetComponent<CharacterController>();
		movementVector = new Vector3();
		timeMoving = Time.time + Random.Range (0f, timeToMove);
	}

	void Update() {
		autoMove();
	}

	private void autoMove() {
		if(Time.time - timeMoving > timeToMove) {
			// Switch strafe directions
			if(direction == 0) {
				direction = 1;
			}
			else if(direction > 0) {
				direction = -1;
			}
			else {
				direction = 1;
			}

			timeMoving = Time.time;
		}

		movementVector.x = direction * strafeSpeed;
		movementVector *= Time.deltaTime;
		characterController.Move(movementVector);
	}
}
