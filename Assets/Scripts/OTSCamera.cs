using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OTSCamera : MonoBehaviour {
	public GameObject target;
	Vector3 offset;
	// Use this for initialization
	void Start () {
		offset = target.transform.position - transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Quaternion rotation = Quaternion.Euler (0, target.transform.eulerAngles.y, 0);
		transform.position = target.transform.position - (rotation * offset);
		transform.rotation = target.transform.rotation;
		//transform.LookAt (target.transform);
	}
}
