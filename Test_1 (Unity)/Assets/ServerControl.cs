using UnityEngine;
using System.Collections;

public class ServerControl : MonoBehaviour {
	private PhotonView pv;
	private bool isServer;

	//To drag player
	private GameObject _target;
	private bool _mouseState;
	
	//To make center of gravity
	private GameObject centerOfGravity = null;

	const string BLACKHOLE = "Blackhole";
	const byte EVENTCODE = 0;
	const byte EVENT_SETBLACKHOLE = 0;
	const byte EVENT_REMOVEBLACKHOLE = 1;

#if UNITY_ANDROID
	private AndroidJavaObject curActivity;
#endif
	bool isMoving=false;
	// Use this for initialization
	void Start () {
		isServer = (bool)PhotonNetwork.player.customProperties ["ISSERVER"];
		pv = gameObject.GetComponent<PhotonView> ();
		Debug.Log ("This object's ID : " + pv.owner.ID);
#if UNITY_ANDROID
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		curActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
#endif
	}

	// Update is called once per frame
	void Update () {
		//If mouse click or touch somewhere, leaf moves to that position.
		if (isServer==true)
		{
			if(Input.GetMouseButtonDown (0)) {
				_target = GetClickedObject(); 
				
				if(_target.tag=="Player") {
					_mouseState=true;
				}
				else if(_target.tag=="Wall" && centerOfGravity==null) {
					Vector3 mousePos=Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
					centerOfGravity=PhotonNetwork.Instantiate(BLACKHOLE, new Vector3(mousePos.x, mousePos.y, 25.0f), Quaternion.identity, 0);
					SetBlackhole();
				}
			}
			else if(Input.GetMouseButtonUp(0)) {
				if(_mouseState==true) {
					_mouseState=false;
					//Animate gameobject's position randomly.
					#if UNITY_ANDROID
					pv.RPC ("Vibrate", _target.GetComponent<PhotonView>().owner, null);
					#endif
					pv.RPC ("SetIsMoving", _target.GetComponent<PhotonView>().owner, false);
					pv.RPC ("ScaleRestore", _target.GetComponent<PhotonView>().owner, null);
					iTween.PunchPosition(_target, new Vector3(50.0f, 50.0f, 0.0f), 1.0f);
					iTween.ShakePosition (_target, new Vector3 (50.0f, 50.0f, 0.0f), 1.0f);
				}
				
				if(centerOfGravity!=null)
				{
					RemoveBlackhole();
					PhotonNetwork.Destroy(centerOfGravity);
					centerOfGravity=null;
				}
			}

			//object dragging
			if(_mouseState==true)
			{
				Vector3 mousePos=Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
				pv.RPC("MoveObjectForce", _target.GetComponent<PhotonView>().owner, mousePos);
			}
		}

		//If object moving, object is zoomed.
		if (isServer==false && isMoving == true) {
			if (gameObject.transform.localScale.x < 50 && gameObject.transform.localScale.y < 50) {
#if UNITY_ANDROID
				gameObject.GetComponent<AndroidPluginManager>().isBeingMovedByServer=true;
#endif
				gameObject.transform.localScale=Vector3.Lerp (gameObject.transform.localScale, gameObject.transform.localScale + new Vector3 (3.0f, 3.0f, 0.0f), 
			              Time.deltaTime * 3.0f);
			}
		}
	}

	//What is the selected object?
	GameObject GetClickedObject() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, 1000.0f)){
			return hit.collider.gameObject;
		}
		return null;
	}

	//Animate Game Object
	void AnimateGameobject ()
	{
		int randomNumber=Random.Range (0, 2);

		switch (randomNumber) {
		case 0:
			iTween.PunchPosition(_target, new Vector3(50.0f, 50.0f, 0.0f), 1.0f);
			break;
		case 1:
			iTween.ShakePosition (_target, new Vector3 (50.0f, 50.0f, 0.0f), 1.0f);
			break;
		default:
			break;
		}
	}

	//Only if it is mine, then move it.
	[PunRPC]
	void MoveObjectForce(Vector3 mousePos) {
		if (pv.isMine) {
			gameObject.transform.position = new Vector3 (mousePos.x, mousePos.y, 25.0f);
			SetIsMoving(true);
		}
	}

	//Make a blackhole. Objects are sucked into the blackhole.
	void SetBlackhole() {
		byte evCode = EVENTCODE;    // my event 0. could be used as "group units"
		byte content = EVENT_SETBLACKHOLE;    // e.g. selected unity 1,2,5 and 10
		bool reliable = true;
		PhotonNetwork.RaiseEvent(evCode, content, reliable, null);
	}

	//Remove the blackhole.
	void RemoveBlackhole() {
		byte evCode = EVENTCODE;    // my event 0. could be used as "group units"
		byte content = EVENT_REMOVEBLACKHOLE;    // e.g. selected unity 1,2,5 and 10
		bool reliable = true;
		PhotonNetwork.RaiseEvent(evCode, content, reliable, null);
	}

	//Vibrate phone.
	[PunRPC]
	void Vibrate() {
		if (pv.isMine) {
			#if UNITY_ANDROID
			curActivity.Call ("vibratePhone", "");
			#endif
		}
	}

	//Is my object moving?
	[PunRPC]
	void SetIsMoving(bool value) {
		if (pv.isMine) {
			isMoving = value;
		}
	}

	//If mouse click ends, restore the scale of object.
	[PunRPC]
	void ScaleRestore() {
		if (pv.isMine) {
			gameObject.transform.localScale=new Vector3(10.0f, 10.0f, 10.0f);
			#if UNITY_ANDROID
			gameObject.GetComponent<AndroidPluginManager>().isBeingMovedByServer=false;
			#endif
		}
	}

	// setup our OnEvent as callback:
	void Awake()
	{
		PhotonNetwork.OnEventCall += this.OnEvent;
	}
	
	// handle events.
	private void OnEvent(byte eventcode, object content, int senderid)
	{
		if (eventcode == 0)
		{
			byte selected = (byte)content;
			Debug.Log ("OnEvent!!!");
			if(pv.isMine)
			{
				switch(selected)
				{
				case EVENT_SETBLACKHOLE:
					Debug.Log ("EVENT_SETBLACKHOLE called!!!");
					GameObject objBlackhole = GameObject.FindGameObjectWithTag (BLACKHOLE);
					gameObject.GetComponent<FauxGravityBody>().attractor=objBlackhole.GetComponent<FauxGravityAttractor>();
					break;
				case EVENT_REMOVEBLACKHOLE:
					Debug.Log ("EVENT_REMOVEBLACKHOLE called!!!");
					Vibrate ();
					gameObject.GetComponent<FauxGravityBody>().attractor=null;
					break;
				default:
					break;
				}

			}
		}
	}
}
