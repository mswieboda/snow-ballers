using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballController : MonoBehaviour {
	public GameObject splattedSnowballPrefab;

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Snowball") {
			return;
		}

		createSplattedSnowball(collision);
		destroy();
	}

	void destroy() {
		Destroy(this.gameObject);
	}

	void createSplattedSnowball(Collision collision) {
		GameObject splat;
		ContactPoint contact = collision.contacts[0];
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
		Vector3 pos = contact.point;

		splat = Instantiate(splattedSnowballPrefab, pos, rot);
		splat.transform.SetParent(collision.transform);
	}
}
