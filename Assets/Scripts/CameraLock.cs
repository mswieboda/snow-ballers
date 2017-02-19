using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLock : MonoBehaviour {
	public GameObject target;
	Vector3 offset;

	void Start () {
//		offset = transform.position - target.transform.position;
		offset = target.transform.position - transform.position;
	}

	void LateUpdate() {
//		transform.position = target.transform.position + offset;

		Quaternion rotation = Quaternion.Euler (0, target.transform.eulerAngles.y, 0);
		transform.position = target.transform.position - (rotation * offset);
		transform.LookAt (target.transform);
	} 
}
