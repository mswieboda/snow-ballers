using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CaptureTheFlaggable {
	void pickUp(Flag flag);
	bool hasFlag { get; set; }
}
