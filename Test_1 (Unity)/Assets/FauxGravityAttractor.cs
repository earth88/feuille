﻿using UnityEngine;
using System.Collections;
	
public class FauxGravityAttractor : MonoBehaviour {

	public float gravity;

	public void Attract(Transform body) {

		Vector3 gravityUp = (body.position - transform.position).normalized;

		Vector3 localUp = body.up;			

		body.GetComponent<Rigidbody>().AddForce(gravityUp * gravity);
		
		Quaternion targetRotation = Quaternion.FromToRotation(localUp,gravityUp) * body.rotation;
		body.rotation = Quaternion.Slerp(body.rotation,targetRotation,50f * Time.deltaTime );
	}    
}