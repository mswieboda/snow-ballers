using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour, Teamable, Spawnable {
	private Team team;

	public void setTeam(Team newTeam) {
		team = newTeam;
	}

	public void changeColor(Color color) {
		GetComponent<MeshRenderer>().material.color = color;
	}

	public void setPosition(Vector3 position) {
		transform.position = position;
	}
}
