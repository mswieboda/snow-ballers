using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GameMode {
	GameObject gameObject { get; }
	bool inProgress { get; }

	void startGameMode(bool isServer);
	void stopGameMode();
	void displayScoreboard();
	void respawnPlayer(Player player, bool isLocal);
}
