using UnityEngine;
using System.Collections;

public class LobbyMgr : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PhotonNetwork.isMessageQueueRunning = true;
	}

	// When ESC key is pressed when player is in lobby, application is quitted.
	void Update() {
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			PhotonNetwork.Disconnect();
			Application.Quit();
		}
	}
}
