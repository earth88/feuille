using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameMgr : MonoBehaviour {

	const string PLAYERCUBE="PlayerCube";
	const string PLAYERSPHERE="LeavesA";
	const int RIGHT_CLICK = 1;
	private bool isServer;

	public AudioClip[] audioclip;
	int currentSongNumber=0;
	const int numberOfTotalSong=3;
	AudioSource audioSource;
	//For LeafControl
	public Dictionary<int, TransformInfo> userTransformInfo;

	void Start() {
		//Reconnect network.
		PhotonNetwork.isMessageQueueRunning = true;

		//isServer? create server object : create client object.
		isServer = (bool)PhotonNetwork.player.customProperties ["ISSERVER"];
		if (isServer == true) {
			Debug.Log ("I am SERVER");
			PhotonNetwork.Instantiate ("ServerObject",
			                           new Vector3(0.0f, 0.0f, 0.0f),
			                           Quaternion.identity,
			                           0);
		} else {
			StartCoroutine (this.CreateLeaf ());
		}

		//For playing background music
		audioSource = gameObject.GetComponent<AudioSource> ();

		//To save each object's position.
		//If it's not my object, refer to userTransformInfo[] and change each position.
		userTransformInfo=new Dictionary<int, TransformInfo>();
		
		PhotonPlayer[] players = PhotonNetwork.playerList;
		foreach (PhotonPlayer player in players) {
			TransformInfo transformInfo = new TransformInfo ();
			if(userTransformInfo.ContainsKey(player.ID)==false)
				userTransformInfo.Add (player.ID, transformInfo);
		}
	}

	void Update() {
		if(isServer==true)
		{
			//Escape the room.
			if (Input.GetKey(KeyCode.Escape)) {
				PhotonNetwork.LeaveRoom();
				StartCoroutine(LoadLobbyForServer());
			}

			//Mouse rightclick, play background music.
			//If playing, then stop the music.
			if (Input.GetMouseButtonUp(RIGHT_CLICK)) {
				if(audioSource.isPlaying==false) {
					AudioPlay(currentSongNumber%numberOfTotalSong);
					currentSongNumber++;
				}
				else {
					audioSource.Stop();
				}
			}
		}
	}

	//Play background music when mouse right button clicks.
	void AudioPlay(int number) {
		audioSource.clip = audioclip [number];
		audioSource.Play ();
	}

	//Exit the room, then load lobby for server.
	IEnumerator LoadLobbyForServer() {
		PhotonNetwork.isMessageQueueRunning = false;
		Application.LoadLevel ("feLobby_Server");
		yield return null;
	}

	//For client, create game object.
	IEnumerator CreateLeaf() {
		string instantiateObjectShape=PhotonNetwork.player.customProperties["SHAPE"].ToString();
		//Cube, Sphere
		switch (instantiateObjectShape) {
		case PLAYERCUBE:
			PhotonNetwork.Instantiate(instantiateObjectShape,
			                          new Vector3(Random.Range(-53.0f, 53.0f), Random.Range(26.0f, 74.0f), 25.0f),
			                          Quaternion.identity,
			                          0);
			break;
		case PLAYERSPHERE:
		default:
			PhotonNetwork.Instantiate(instantiateObjectShape,
			                          new Vector3(Random.Range(-53.0f, 53.0f), Random.Range(26.0f, 74.0f), 30.0f),
			                          Quaternion.identity,
			                          0);
			break;
		}

		yield return null;
	}

	//When player comes to the room, add userTransformInfo.
	void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
		TransformInfo transformInfo = new TransformInfo ();
		if(userTransformInfo.ContainsKey(newPlayer.ID)==false)
			userTransformInfo.Add (newPlayer.ID, transformInfo);
	}

	//When player leaves the room, remove userTransformInfo.
	void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer) {
		if(userTransformInfo.ContainsKey(disconnectedPlayer.ID)==true)
			userTransformInfo.Remove(disconnectedPlayer.ID);
	}
	
	public class TransformInfo {
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
	}
}


