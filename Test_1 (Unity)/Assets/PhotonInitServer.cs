using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//All things are same with PhotonInit, but this is server version.
//Please check PhotonInit.

public class PhotonInitServer : MonoBehaviour {

	public string version="v1.0";
	public InstantGuiButton backgroundSettingButton;
	ExitGames.Client.Photon.Hashtable playerPropertyHashtable;
	ExitGames.Client.Photon.Hashtable roomPropertyHashtable;

	private string stringBackgrounds="Backgrounds/";

	void Awake() {
		PhotonNetwork.ConnectUsingSettings (version);
		playerPropertyHashtable = new ExitGames.Client.Photon.Hashtable ();
		roomPropertyHashtable = new ExitGames.Client.Photon.Hashtable ();
	}

	void OnFailedToConnectToPhoton(DisconnectCause cause) {
		StartCoroutine (ReconnectServer ());
	}

	IEnumerator ReconnectServer() {
		yield return new WaitForSeconds (5);
		PhotonNetwork.ConnectUsingSettings (version);
	}

	void OnGUI() {
		//Server connection status
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	//On Joined Lobby Callback method
	void OnJoinedLobby() {
		Debug.Log ("Entered Lobby!");
		StopCoroutine (ReconnectServer ());

		//Initialize UserID, shape, texture.
		backgroundSettingButton.disabled = false;

		//I am SERVER
		playerPropertyHashtable["ISSERVER"] = true;
		setBackground (stringBackgrounds + "blue", false);
	}

	//무작위 룸 접속에 실패한 경우 호출되는 콜백함수
	void OnPhotonRandomJoinFailed() {
		Debug.Log ("No rooms!");
		//Make a room
		RoomOptions roomOptions = new RoomOptions ();
		roomOptions.customRoomProperties = roomPropertyHashtable;
		PhotonNetwork.CreateRoom ("MyRoom", roomOptions, null);
	}

	void OnJoinedRoom() {
		Debug.Log ("Enter Room");
		Room curRoom = PhotonNetwork.room;

		curRoom.SetCustomProperties (roomPropertyHashtable);
		StartCoroutine (this.LoadStage ());
	}

	void OnPhotonCreateRoomFailed(object[] error) {
		Debug.Log (error [0].ToString ());
		Debug.Log (error[1].ToString());
	}

	public void OnClickJoinRoom() {
		PlayerPrefs.SetString ("USER_ID", "SERVER");
		PhotonNetwork.player.SetCustomProperties (playerPropertyHashtable);
		PhotonNetwork.JoinRandomRoom ();
	}

	public void OnClickBackground(string background) {
		Debug.Log ("OnClickBackground Called." + background);
		setBackground (background, false);
	}

	public void SetCustomBackgroundFromAndroid(string path) {
		setBackground (path, true);
	}

	IEnumerator LoadStage() {
		PhotonNetwork.isMessageQueueRunning = false;
		Application.LoadLevel ("feStage");
		yield return null;
	}

	void setBackground(string path, bool isCustomTexture) {
		roomPropertyHashtable["ISCUSTOMBACK"] = isCustomTexture;
		roomPropertyHashtable["BACKGROUND"] = path;
	}
}

