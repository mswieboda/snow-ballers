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

		ContactPoint contact = collision.contacts[0];
		Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
		Vector3 position = contact.point;

		CmdCreateSplattedSnowball(position, rotation);

		destroy();
	}

	void destroy() {
		Destroy(this.gameObject);
	}

	[Command]
	void CmdCreateSplattedSnowball(Vector3 position, Quaternion rotation) {
		GameObject splat = Instantiate(splattedSnowballPrefab, position, rotation);

		// TODO: not sure how to do this since we can't pass a Collision or Transform object
		// via the arguments for the [Command] calls, see:
		// https://docs.unity3d.com/Manual/UNetActions.html
		// splat.transform.SetParent(collision.transform);

		NetworkServer.Spawn(splat);
	}
}
