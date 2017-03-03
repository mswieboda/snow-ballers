using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Player : Teamable, Spawnable, CaptureTheFlaggable {
	// TODO: custom player methods like hitBySnowball(GameObject), pickUp(Flag/GameObject)
	Transform transform { get; }
}
