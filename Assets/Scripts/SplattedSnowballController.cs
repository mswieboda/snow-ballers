using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplattedSnowballController : MonoBehaviour {

	public int maxSplats = 100;

	private static List<GameObject> splats = new List<GameObject>();

	// Use this for initialization
	void Start () {
		splats.Add(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if (splats.Count > maxSplats) {
			destroyASplat();
		}
	}

	static void destroyASplat() {
		GameObject splat = splats[0];
		splats.RemoveAt(0);
		Destroy(splat);
	}
}
