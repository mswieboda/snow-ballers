using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Player {
	void setTeam(Team team);
	void setPosition(Vector3 position);
	void changeColor(Color color);
}
