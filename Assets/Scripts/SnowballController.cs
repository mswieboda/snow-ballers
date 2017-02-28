using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnowballController : NetworkBehaviour {
	public GameObject splattedSnowballPrefab;

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Snowball") {
			return;
		}

		ContactPoint contact;
		Quaternion rotation;
		Vector3 position;
		GameObject splat;

		contact = collision.contacts[0];
		rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
		position = contact.point;

		splat = Instantiate(splattedSnowballPrefab, position, rotation);

		// Attach to collision object
		splat.transform.SetParent(collision.transform);

		NetworkServer.Spawn(splat);

		destroy();
	}

	void destroy() {
		Destroy(this.gameObject);
	}
}
