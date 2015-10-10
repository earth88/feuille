using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
public class FauxGravityBody : MonoBehaviour {
	
	public FauxGravityAttractor attractor;
	private Transform myTransform;
	
	void Start () {
		GetComponent<Rigidbody>().useGravity = false;
		
		myTransform = transform;
//		attractor = GameObject.Find("Planet").GetComponent<FauxGravityAttractor>();
	}
	
	void FixedUpdate () {
		if (attractor){
			attractor.Attract(myTransform);
		}
	}	
}