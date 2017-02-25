using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OTSCamera : MonoBehaviour {
	public GameObject target;
	private Vector3 offset;

	void Start () {
		offset = target.transform.position - transform.position;
	}

	void LateUpdate () {
		Quaternion rotation = Quaternion.Euler (0, target.transform.eulerAngles.y, 0);
		transform.position = target.transform.position - (rotation * offset);
		transform.rotation = target.transform.rotation;
	}
}
