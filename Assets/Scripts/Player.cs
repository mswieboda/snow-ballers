using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Player : Teamable, Spawnable {
	// TODO: custom player methods like hitBySnowball(GameObject), pickUp(Flag/GameObject)
	void pickUp(Flag flag);
}
