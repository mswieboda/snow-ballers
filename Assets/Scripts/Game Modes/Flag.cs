using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour, Teamable, Spawnable {

	public Player holder { get; set; }

	private Vector3 basePosition;
	private Rigidbody rb;

	void Awake() {
		rb = GetComponent<Rigidbody>();
	}

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

	public void holdBy(Player player, Vector3 localPosition) {
		holder = player;

		clearForce();
		disableGravity();

		transform.SetParent(player.transform);
		transform.localPosition = localPosition;
	}

	public void dropFromHolder() {
		if (holder != null) {
			holder.heldFlag = null;
			holder = null;
		}

		transform.SetParent(team.transform);
		enableGravity();
	}

	public void throwFlag(Vector3 position, Vector3 force) {
		dropFromHolder();
		setPosition(position);
		rb.velocity = force;
	}

	public void returnToBase() {
		dropFromHolder();
		setPosition(basePosition);
	}

	public void disableGravity() {
		rb.useGravity = false;
	}

	public void enableGravity() {
		rb.useGravity = true;
	}

	public void clearForce() {
		rb.velocity = Vector3.zero;
	}
}
