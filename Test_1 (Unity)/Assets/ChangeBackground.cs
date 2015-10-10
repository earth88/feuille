using UnityEngine;
using System.Collections;

public class ChangeBackground : MonoBehaviour {
	private PhotonView pv;
	GameObject alphaWall;
	Room curRoom;
	// Use this for initialization
	void Start () {
		alphaWall = GameObject.Find ("AlphaWall") as GameObject;
		pv = GetComponent<PhotonView> ();
		curRoom = PhotonNetwork.room;
		bool isCustomBack = System.Convert.ToBoolean (curRoom.customProperties ["ISCUSTOMBACK"].ToString());
		string backgroundOfNewUser = curRoom.customProperties ["BACKGROUND"].ToString();
		pv.RPC ("SetBackground", PhotonTargets.All, backgroundOfNewUser, isCustomBack);
	}

	//Set background. Download texture from Amazon server.
	[PunRPC]
	public void SetBackground(string background, bool isCustomBack) {
		if (isCustomBack == true) {
			StartCoroutine(DownloadBackground(background));
		} else {
			Texture texture=Resources.Load (background) as Texture;
			alphaWall.GetComponent<Renderer>().material.mainTexture=texture;
		}
	}

	IEnumerator DownloadBackground(string url) {
		string escapeUriString = System.Uri.EscapeUriString (url);
		WWW www = new WWW (escapeUriString); 
		
		// Wait for download to complete 
		yield return www; 
		
		// assign texture 
		alphaWall.GetComponent<Renderer>().material.mainTexture = www.texture; 
	}
}
