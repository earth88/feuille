using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AndroidPluginManager : MonoBehaviour
{
#if UNITY_ANDROID
	private AndroidJavaObject curActivity;
	private AndroidJavaClass firstPluginJc;
	private PhotonView photonView;
	Vector3 transform_vector;
	public bool isChangeLeafPositionEnabled=true;
	Vector3 scaleVector;
	public float y_rotation;
	public bool isBeingMovedByServer=false;

	const float ORIGINAL_SIZE = 10.0f;
	const float ALPHA = 2.0f;
	const float DIVIDEBY = 30.0f;
	void Start()
	{
		photonView = gameObject.GetComponent<PhotonView> ();
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		curActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
		transform_vector = new Vector3 (0, 0, 0);
		scaleVector=new Vector3();
		Debug.Log ("AndroidPluginManager this called!!!!!");
		if (photonView.isMine == true) {
			SetCurrentObjectNameToAndroid (gameObject.name);
		}
	}

	//@Deprecated.
	public void AccelCamera(string parameter)
	{
//		main_camera_script.CameraShakeEffect();
	}

	//@Deprecated.
	public void CameraRotation(string parameter)
	{
//		if (Network.isServer) {
//			var value_y = System.Convert.ToSingle (parameter);
//			if(value_y!=0)
//			{
//				y_rotation=value_y;
//			}
//			main_camera_script.CameraRotation (value_y);
//		}
	}

	//Send my object name to android phone.
	public void SetCurrentObjectNameToAndroid(string name) {
		curActivity.Call ("setCurrentObjectName", name);
	}

	//Change game object's position.
	public void ChangeLeafPosition(string parameter){
		if (photonView.isMine==true && isChangeLeafPositionEnabled == true) {
			string[] vector_array = parameter.Split (',');
			var value_x = System.Convert.ToSingle (vector_array [0]);
			var value_y = System.Convert.ToSingle (vector_array [1]);
			var value_z=0;

			transform_vector.Set (value_x*100.0f, value_y*100.0f, 0.0f);

			//Control object's force
			
			//가속도계의 절대값이 2보다 작거나 같으면 물체는 정지함
			if(Mathf.Abs(value_x)<=2 && Mathf.Abs (value_y)<=2)
			{
				gameObject.GetComponent<Rigidbody>().velocity=new Vector3(0.0f, 0.0f, 0.0f);
			}
			//아닐 경우에는 100배의 힘을 작용시켜서 움직임
			else{
			gameObject.GetComponent<Rigidbody>().AddForce(transform_vector, ForceMode.Impulse);
			}

			if(isBeingMovedByServer==false) {
				//Control object's scale 
				scaleVector.Set(ORIGINAL_SIZE+ALPHA*Mathf.Abs(value_x),
				                ORIGINAL_SIZE+ALPHA*Mathf.Abs(value_y),
				                ORIGINAL_SIZE);
				gameObject.transform.localScale=scaleVector;
			}
		}
		else {
			return;
		}
	}

	//When leaf is stopped, stay 2 seconds then start again.
	public IEnumerator StopLeafCallByLeafControl() {
		isChangeLeafPositionEnabled = false;
		yield return new WaitForSeconds (2);
		isChangeLeafPositionEnabled = true;
	}

	//@depreacted.
	public void SetHeartrate(string parameter){
//		var heartrate = System.Convert.ToSingle (parameter);
//		wall_script.SetHeartrate (heartrate);
	}

	//@deprecated.
	public void CancelHeartrate(string parameter) {
//		wall_script.CancelHeartrate ();
	}

	//Vibrate my phone.
	public void VibratePhone()
	{
		curActivity.Call ("vibratePhone", "");
	}
#endif
}