using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Text;

public class LeafControl : MonoBehaviour {
	private PhotonView pv;
	private int cnt;
	private AndroidPluginManager androidPluginManager;
	private Vector3 temp_add_force_vector;
	private Rigidbody _rigidbody;
	private Transform _transform;

	private bool isServer;

	GameMgr gameMgr;
	int ownerID;

	void Start()
	{
		//PhotonNetwork sends 30 times in a second.
		PhotonNetwork.sendRate = 30;
		PhotonNetwork.sendRateOnSerialize = 30;
		pv = GetComponent<PhotonView> ();

		//All player must renew all object's texture.
		pv.RPC ("SetTextureOfPlayer", PhotonTargets.All, null);

		androidPluginManager = GetComponent<AndroidPluginManager> ();
		_rigidbody = GetComponent<Rigidbody> ();
		_transform = GetComponent<Transform> ();

		if (pv.isMine == true) {

		} else {
			_rigidbody.isKinematic=true;
		}

		isServer = (bool)PhotonNetwork.player.customProperties ["ISSERVER"];
		GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionZ;
		gameMgr = GameObject.Find ("GameManager").GetComponent<GameMgr> ();
		ownerID = gameObject.GetComponent<PhotonView> ().owner.ID;
	}

	void Update() {
		if (pv.isMine) {
		} else {
			//If it is not mine, get transform data from userTransformInfo[]
			_transform.position=Vector3.Lerp(_transform.position, gameMgr.userTransformInfo[ownerID].position, Time.deltaTime*30.0f);
			_transform.rotation=Quaternion.Slerp(_transform.rotation, gameMgr.userTransformInfo[ownerID].rotation, Time.deltaTime*30.0f);
			_transform.localScale=Vector3.Lerp(_transform.localScale, gameMgr.userTransformInfo[ownerID].scale, Time.deltaTime*30.0f);
		}

#if UNITY_ANDROID
		//If Esc key is pressed, leave the room.
		if(Input.GetKey(KeyCode.Escape)) {
			PhotonNetwork.LeaveRoom();
			StartCoroutine(LoadLobby());
		}
#endif
	}

	//Leave the room.
	IEnumerator LoadLobby() {
		Destroy(gameObject);
		#if UNITY_ANDROID
		//cancel mTimer (Android)
		if (pv.isMine == true) {
			androidPluginManager.SetCurrentObjectNameToAndroid ("");
		}
		#endif
		PhotonNetwork.isMessageQueueRunning = false;
		Application.LoadLevel ("feLobby_Client");
		yield return null;
	}

	//All player must renew object's texture when new player comes.
	[PunRPC]
	void SetTextureOfPlayer() {
		GameObject[] leaves = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject leaf in leaves) {
			string texture=leaf.GetComponent<PhotonView>().owner.customProperties["TEXTURE"].ToString();
			string url=leaf.GetComponent<PhotonView>().owner.customProperties["URL"].ToString();
			//If texture exists, load it.
			//Else, download from Amazon server.
			if(System.String.IsNullOrEmpty(url)==true)
			{
				leaf.GetComponent<Renderer>().material.mainTexture=Resources.Load (texture) as Texture;
			}
			else
			{
				StartCoroutine(DownloadTexture(leaf, url));
			}
		}
	}

	//Download from Amazon server.
	IEnumerator DownloadTexture(GameObject leaf, string url) {
		string escapeUriString = System.Uri.EscapeUriString (url);
		WWW www = new WWW (escapeUriString); 
		
		// Wait for download to complete 
		yield return www; 
		
		// assign texture 
		leaf.GetComponent<Renderer>().material.mainTexture = www.texture; 
	}

	//Synchronize object's transform.
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		//로컬 플레이어의 위치 정보 송신
		if (stream.isWriting) {
			stream.SendNext (_transform.position);
			stream.SendNext (_transform.rotation);
			stream.SendNext (_transform.localScale);
		}
		//원격 플레이어의 위치 정보 수신
		else {
			gameMgr.userTransformInfo[info.sender.ID].position=(Vector3)stream.ReceiveNext();
			gameMgr.userTransformInfo[info.sender.ID].rotation=(Quaternion)stream.ReceiveNext();
			gameMgr.userTransformInfo[info.sender.ID].scale=(Vector3)stream.ReceiveNext();
		}
	}

}
