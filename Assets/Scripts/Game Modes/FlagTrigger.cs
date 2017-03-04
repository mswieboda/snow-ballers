using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagTrigger : MonoBehaviour, Teamable {
	public Team team { get; set; }
	public Flag teamFlag { get; set; }

	public void changeColor(Color color) {
	}

	public void triggeredBy(Player player) {
		if (team == player.team && teamFlag.isAtBase() && player.hasFlag()) {
			team.addScore(1);
			player.heldFlag.returnToBase();
		}
	}
}
