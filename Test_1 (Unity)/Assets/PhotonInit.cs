using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotonInit : MonoBehaviour {

	public string version="v1.0";
	public InstantGuiInputText userId;
	public InstantGuiButton textureSettingButton;
	public InstantGuiButton backgroundSettingButton;
	public InstantGuiButton shapeSettingButton;
	ExitGames.Client.Photon.Hashtable playerPropertyHashtable;
	ExitGames.Client.Photon.Hashtable roomPropertyHashtable;
#if UNITY_ANDROID
	AndroidJavaClass androidJavaClass;
	AndroidJavaObject currentActivity;
#endif
	private string stringBackgrounds="Backgrounds/";

	void Awake() {
		PhotonNetwork.ConnectUsingSettings (version);
		playerPropertyHashtable = new ExitGames.Client.Photon.Hashtable ();
		roomPropertyHashtable = new ExitGames.Client.Photon.Hashtable ();
#if UNITY_ANDROID
		androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		currentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
#endif
	}

	//On failed to connect to Photon, reconnect to server.
	void OnFailedToConnectToPhoton(DisconnectCause cause) {
		StartCoroutine (ReconnectServer ());
	}

	//Retry every 5 seconds.
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
		//TODO Am I Computer?

		//Initialize UserID, shape, texture.
		userId.text = GetUserID ();
		shapeSettingButton.disabled = false;
		textureSettingButton.disabled=false;
		backgroundSettingButton.disabled = false;

		//I am CLIENT
		playerPropertyHashtable["ISSERVER"] = false;
		setShape ("LeavesA", false);
		setTexture ("LeafA 1", false);
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
		//Set roomPropertyHashtable to the current room.
		curRoom.SetCustomProperties (roomPropertyHashtable);
		StartCoroutine (this.LoadStage ());
	}

	//For debugging.
	void OnPhotonCreateRoomFailed(object[] error) {
		Debug.Log (error [0].ToString ());
		Debug.Log (error[1].ToString());
	}

	//On click join room, set player's preference such as USER_ID and playerPropertyHashtable.
	public void OnClickJoinRoom() {
		PhotonNetwork.player.name = userId.text;
		PlayerPrefs.SetString ("USER_ID", userId.text);
		PhotonNetwork.player.SetCustomProperties (playerPropertyHashtable);
		PhotonNetwork.JoinRandomRoom ();
	}

	//For UI. Texture setting.
	public void OnClickLeaves(string leaf) {
		Debug.Log ("OnClickLeaves Called." + leaf);
		setTexture (leaf,false);
	}

	//For UI. Background setting.
	public void OnClickBackground(string background) {
		Debug.Log ("OnClickBackground Called." + background);
		setBackground (background, false);
	}

	//For UI. Shape setting.
	public void OnClickShape(string shape) {
		Debug.Log ("OnClickShape Called." + shape);
		setShape (shape, false);
	}
	//안드로이드로부터 받아온 이미지 파일의 경로를 저장
	public void SetCustomTextureFromAndroid(string path) {
		setTexture (path, true);
	}

	//@Deprecated.
	public void SetCustomBackgroundFromAndroid(string path) {
		setBackground (path, true);
	}

	//Get user ID from text form.
	string GetUserID() {
		string userId = PlayerPrefs.GetString ("USER_ID");
		
		if (string.IsNullOrEmpty (userId)) {
			userId = "USER_" + Random.Range (0, 999);
		}
		return userId;
	}

	//Join the room. Load feStage.
	IEnumerator LoadStage() {
		PhotonNetwork.isMessageQueueRunning = false;
		Application.LoadLevel ("feStage");
		yield return null;
	}

	//setTexture~setShape = set user's preference through the hashtable.
	void setTexture(string leaf, bool isCustomTexture) {
		if (isCustomTexture == false) {
			playerPropertyHashtable ["URL"] = "";
			playerPropertyHashtable ["TEXTURE"] = leaf;
		} else {
			playerPropertyHashtable ["URL"] = leaf;
			playerPropertyHashtable ["TEXTURE"] = "";
		}
	}

	void setBackground(string path, bool isCustomTexture) {
		roomPropertyHashtable["ISCUSTOMBACK"] = isCustomTexture;
		roomPropertyHashtable["BACKGROUND"] = path;
	}

	void setShape(string path, bool isCustomShape) {
		if (isCustomShape == false) {
			playerPropertyHashtable ["SHAPE"] = path;
		} else {
			//TODO
		}
	}

	//If ... is clicked, load android gallery.
	public void OnClickDots(string callMethod) {
#if UNITY_ANDROID
		currentActivity.Call (callMethod, gameObject.name.ToString());
#endif
	}

}

