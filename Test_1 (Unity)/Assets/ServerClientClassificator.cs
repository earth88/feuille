using UnityEngine;
using System.Collections;

public class ServerClientClassificator : MonoBehaviour {

	//If android, client.
	//Else, server.
	void Awake () {
		if (Application.platform == RuntimePlatform.Android) {
			Application.LoadLevel ("feLobby_Client");
		} else {
			Application.LoadLevel ("feLobby_Server");
		}
	}

}
