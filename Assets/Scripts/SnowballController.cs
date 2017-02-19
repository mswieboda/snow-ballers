using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballController : MonoBehaviour {
	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Wall") {
			Destroy (this.gameObject);
		}
	}
}
