using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {
	public float speed;
	public PlayerAnimationController animationControl;
	public SplineInterpolator interpolator;

	private Rigidbody body;
	private bool moving; 

	void Start () {
		body = GetComponent<Rigidbody> ();
		moving = false;
		previousPosition = body.position; 
		jumpTimeCounter = jumpTime;
	}
	void Update()
	{ 
		var colls = Physics.OverlapSphere (groundCheck.position,checkRadius, whatIsGround);  
		grounded = colls.Length > 0;
		if(grounded)
		{ 
			jumpTimeCounter = jumpTime;
		}
		var collsTop = Physics.OverlapSphere (topCheck.position, checkRadius, whatIsGround);
		if (collsTop.Length > 0){
			jumpTimeCounter = 0;
		}
	}
	void FixedUpdate () {
		ControllPlayer ();
	}      
	private Vector3 previousPosition, direction; 
	private float previousHorizontal = 0.0f;
	void ControllPlayer()
	{  
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		if (moveHorizontal != 0) {
			interpolator.Move (Input.GetKey (KeyCode.LeftShift) ? 2 * speed * moveHorizontal : speed * moveHorizontal);  
			previousHorizontal = moveHorizontal;  
			if ((body.position-previousPosition).magnitude>0.1f) {
				var newRot = Quaternion.LookRotation (body.position - previousPosition);
				newRot.Set (0.0f, newRot.y, 0.0f, newRot.w);
				//transform.rotation = Quaternion.Lerp (body.rotation, newRot,1); 
				transform.rotation = newRot;
				previousPosition = body.position; 
			} 
		} 
		AnimateMovement (moveHorizontal); 
		Attack ();  
		Jump ();
	}

	void Attack ()
	{
		if (Input.GetButtonDown ("Fire1")) {
			animationControl.Strike ();
		}
	}
	public float jumpForce=2;
	public float jumpTime;
	private float jumpTimeCounter; 
	public bool grounded;
	public LayerMask whatIsGround;
	public bool stoppedJumping; 
	public Transform groundCheck;
	public Transform topCheck;
	public float checkRadius;
	private void Jump(){
		if(Input.GetKeyDown(KeyCode.Space) )
		{
			//and you are on the ground...
			if(grounded)
			{
				//jump!
				body.velocity = new Vector3 (body.velocity.x, jumpForce, body.velocity.z);
				stoppedJumping = false;
			}
		}

		//if you keep holding down the mouse button...
		if((Input.GetKey(KeyCode.Space)) && !stoppedJumping)
		{
			//and your counter hasn't reached zero...
			if(jumpTimeCounter > 0)
			{
				//keep jumping!
				body.velocity = new Vector3 (body.velocity.x, jumpForce, body.velocity.z);
				jumpTimeCounter -= Time.deltaTime;
			}
		}


		//if you stop holding down the mouse button...
		if(Input.GetKeyUp(KeyCode.Space))
		{
			//stop jumping and set your counter to zero.  The timer will reset once we touch the ground again in the update function.
			jumpTimeCounter = 0;
			stoppedJumping = true;
		}
	}

	void AnimateMovement (float moveHorizontal)
	{
		if (!moving && moveHorizontal != 0) {  
				animationControl.Walk ();
			moving = true;
		}
		if (moving && moveHorizontal == 0) {
			animationControl.OtherIdle ();
			moving = false;
		}
	}
}
