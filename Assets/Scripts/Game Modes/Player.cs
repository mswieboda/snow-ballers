using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Player : Teamable, Spawnable, CaptureTheFlaggable {
	Transform transform { get; }
	void spawnInitialization();
}
