using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Teamable {
	Team team { get; set; }
	void changeColor(Color color);
}
