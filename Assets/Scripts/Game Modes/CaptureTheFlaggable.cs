using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CaptureTheFlaggable {
	Flag heldFlag { get; set; }
	void pickUp(Flag flag);
	bool hasFlag();
	void hitBySnowball();
}
