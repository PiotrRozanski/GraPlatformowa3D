using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour {
	public float rotationSpeed = 15;
	public Transform target;
	Vector3 offset;
	void Start () {
		offset = target.position - transform.position;
	}
	private Quaternion _lookRotation;
	private Vector3 _direction;

	// Update is called once per frame
	void Update()
	{
		//find the vector pointing from our position to the target
		_direction = (target.position - transform.position).normalized;

		//create the rotation we need to be in to look at the target
		_lookRotation = Quaternion.LookRotation(_direction);

		//rotate us over time according to speed until we are in the required rotation
		transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * rotationSpeed);
		transform.position = target.position -offset; 
	}
}
