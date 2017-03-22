using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {
	public float speed;
	public PlayerAnimationController animationControl;

	private Rigidbody body;
	private bool moving;
	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody> ();
		moving = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		ControllPlayer ();
	}     
	void ControllPlayer()
	{
		float moveHorizontal = -Input.GetAxisRaw ("Horizontal");
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, 0.0f);
		Vector3 lookAtMe = new Vector3 (0.0f, 0.0f, 1.0f);
		transform.Translate (movement * speed * Time.deltaTime, Space.World);
		if (moveHorizontal != 0) {
			var newRot = Quaternion.LookRotation(movement);
			transform.rotation = Quaternion.Lerp(transform.rotation, newRot, speed);  
		} else {
			var newRot = Quaternion.LookRotation(lookAtMe);
			transform.rotation = Quaternion.Lerp(transform.rotation, newRot, 0.2f);  
		}

		if (!moving && moveHorizontal!=0  ) {
			animationControl.Walk();
			moving = true;
		}
		if (moving && moveHorizontal == 0) {
			animationControl.OtherIdle ();
			moving = false;
		}

		if(Input.GetButtonDown ("Fire1"))
		{ 
			animationControl.Strike ();

		}  
		if (Input.GetKeyDown (KeyCode.Space)){

			body.AddForce(new Vector3(0, 2, 0), ForceMode.Impulse);
		}
	}
}
