using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour, Teamable, Spawnable {

	public Player holder { get; set; }

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

	public bool isHeld() {
		return holder != null;
	}

	public void dropFromHolder() {
		if (holder != null) {
			holder.heldFlag = null;
			holder = null;
		}

		transform.SetParent(team.transform);
	}

	public void returnToBase() {
		// TODO: Turn flag gravity off
		dropFromHolder();
		setPosition(basePosition);
	}

	void OnCollisionEnter(Collision collision) {
		Debug.Log("Something hit the flag!");
		Player player = collision.gameObject.GetComponent<Player>();
		if (player != null) {
			Debug.Log("collided with Player");
			Collider collider = GetComponent<Collider>();
			Physics.IgnoreCollision(collider, collision.collider);
			return;
		}
	}
}
