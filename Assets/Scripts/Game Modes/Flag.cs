using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour, Teamable, Spawnable {
	
	private Vector3 basePosition;

	private Team mTeam;
	public Team team { 
		get { return mTeam; }
		set
		{
			mTeam = value;
			changeColor(mTeam.color);
		}
	}

	public void changeColor(Color color) {
		MeshRenderer[] meshes = transform.GetComponentsInChildren<MeshRenderer>();

		foreach(MeshRenderer mesh in meshes) {
			mesh.material.color = color;
		}
	}

	public void setPosition(Vector3 position) {
		transform.position = position;
	}

	public void setBasePosition(Vector3 position) {
		basePosition = position;
	}

	public bool isAtBase() {
		return transform.position == basePosition;
	}

	public void returnToBase() {
		setPosition(basePosition);
		transform.SetParent(team.transform);
	}
}
