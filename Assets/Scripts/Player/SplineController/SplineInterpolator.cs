using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum eEndPointsMode { AUTO, AUTOCLOSED, EXPLICIT }
public enum eWrapMode { ONCE, LOOP }
public delegate void OnEndCallback();

public class SplineInterpolator : MonoBehaviour
{
	eEndPointsMode mEndPointsMode = eEndPointsMode.AUTO;

	internal class SplineNode
	{
		internal Vector3 Point;
		internal Quaternion Rot;
		internal float Distance;
		internal Vector2 EaseIO;

		internal SplineNode(Vector3 p, Quaternion q, float t, Vector2 io) { Point = p; Rot = q; Distance = t; EaseIO = io; }
		internal SplineNode(SplineNode o) { Point = o.Point; Rot = o.Rot; Distance = o.Distance; EaseIO = o.EaseIO; }
	}

	List<SplineNode> mNodes = new List<SplineNode>();
	string mState = "";
	bool mRotations;

	OnEndCallback mOnEndCallback;



	void Awake()
	{
		Reset();
	}

	public void StartInterpolation(OnEndCallback endCallback, bool bRotations, eWrapMode mode)
	{
		if (mState != "Reset")
			throw new System.Exception("First reset, add points and then call here");

		mState = mode == eWrapMode.ONCE ? "Once" : "Loop";
		mRotations = bRotations;
		mOnEndCallback = endCallback;

		SetInput();
	}

	public void Reset()
	{
		mNodes.Clear();
		mState = "Reset";
		mCurrentIdx = 1;
		mCurrentDistance = 0;
		mRotations = false;
		mEndPointsMode = eEndPointsMode.AUTO;
	}

	public void AddPoint(Vector3 pos, Quaternion quat, float timeInSeconds, Vector2 easeInOut)
	{
		if (mState != "Reset")
			throw new System.Exception("Cannot add points after start");

		mNodes.Add(new SplineNode(pos, quat, timeInSeconds, easeInOut));
	}


	void SetInput()
	{
		if (mNodes.Count < 2)
			throw new System.Exception("Invalid number of points");

		if (mRotations)
		{
			for (int c = 1; c < mNodes.Count; c++)
			{
				SplineNode node = mNodes[c];
				SplineNode prevNode = mNodes[c - 1];

				// Always interpolate using the shortest path -> Selective negation
				if (Quaternion.Dot(node.Rot, prevNode.Rot) < 0)
				{
					node.Rot.x = -node.Rot.x;
					node.Rot.y = -node.Rot.y;
					node.Rot.z = -node.Rot.z;
					node.Rot.w = -node.Rot.w;
				}
			}
		}

		if (mEndPointsMode == eEndPointsMode.AUTO)
		{
			mNodes.Insert(0, mNodes[0]);
			mNodes.Add(mNodes[mNodes.Count - 1]);
		}
		else if (mEndPointsMode == eEndPointsMode.EXPLICIT && (mNodes.Count < 4))
			throw new System.Exception("Invalid number of points");
	}

	void SetExplicitMode()
	{
		if (mState != "Reset")
			throw new System.Exception("Cannot change mode after start");

		mEndPointsMode = eEndPointsMode.EXPLICIT;
	}

	public void SetAutoCloseMode(float joiningPointTime)
	{
		if (mState != "Reset")
			throw new System.Exception("Cannot change mode after start");

		mEndPointsMode = eEndPointsMode.AUTOCLOSED;

		mNodes.Add(new SplineNode(mNodes[0] as SplineNode));
		mNodes[mNodes.Count - 1].Distance = joiningPointTime;

		Vector3 vInitDir = (mNodes[1].Point - mNodes[0].Point).normalized;
		Vector3 vEndDir = (mNodes[mNodes.Count - 2].Point - mNodes[mNodes.Count - 1].Point).normalized;
		float firstLength = (mNodes[1].Point - mNodes[0].Point).magnitude;
		float lastLength = (mNodes[mNodes.Count - 2].Point - mNodes[mNodes.Count - 1].Point).magnitude;

		SplineNode firstNode = new SplineNode(mNodes[0] as SplineNode);
		firstNode.Point = mNodes[0].Point + vEndDir * firstLength;

		SplineNode lastNode = new SplineNode(mNodes[mNodes.Count - 1] as SplineNode);
		lastNode.Point = mNodes[0].Point + vInitDir * lastLength;

		mNodes.Insert(0, firstNode);
		mNodes.Add(lastNode);
	}

	float mCurrentDistance;
	int mCurrentIdx = 1; 

	public void Move(float speed)
	{
		if (mState == "Reset" || mState == "Stopped" || mNodes.Count < 4 )
			return;  
		float tempDistance = mCurrentDistance + speed * Time.deltaTime; 
		int tempIndex = mCurrentIdx;

		if (tempDistance <= 0) {
			tempDistance = mNodes [mNodes.Count - 2].Distance;
		}   else if (tempDistance >= mNodes[tempIndex + 1].Distance)
		{
			if (tempIndex < mNodes.Count - 3)
			{
				tempIndex++;
			}
			else
			{
				if (mState != "Loop")
				{
					mState = "Stopped";

					// We stop right in the end point
					transform.position = mNodes[mNodes.Count - 2].Point;

					if (mRotations)
						//transform.rotation = mNodes[mNodes.Count - 2].Rot;

					// We call back to inform that we are ended
					if (mOnEndCallback != null)
						mOnEndCallback();
				}
				else
				{
					tempIndex = 1;
					tempDistance = 0;
				}
			}
		}
		if (tempDistance < mNodes [tempIndex].Distance) {
			if (tempIndex > 1) {
				tempIndex--;
			} else { 
//				tempIndex = mNodes.Count - 2;
//				tempDistance = mNodes [mNodes.Count - 1].Distance;
				tempIndex = 1;
				tempDistance = 0;
			}
		
		} 
		if (mState != "Stopped") {
			// Calculates the t param between 0 and 1
			float param = (tempDistance - mNodes [tempIndex].Distance) / (mNodes [tempIndex + 1].Distance - mNodes [tempIndex].Distance);

			// Smooth the param
			param = MathUtils.Ease (param, mNodes [tempIndex].EaseIO.x, mNodes [tempIndex].EaseIO.y);
			var y = transform.position.y;
			Vector3 vec = GetHermiteInternal (tempIndex, param);
			vec.y = y; 
 

			var colls = Physics.OverlapSphere (frontCheck.position, frontCheckRadius, whatIsGround); 
			bool  barrierAhead = colls.Length > 0;
			if (barrierAhead && blockedFrom==BlockedFrom.None) {
				if (speed > 0) {
					blockedFrom = BlockedFrom.Right;
					Debug.Log ("Zablokowalem po prawej");
				} else if (speed<0){
					blockedFrom = BlockedFrom.Left;
					Debug.Log ("Zablokowalem po lewej");
				}
			}
			if (blockedFrom == BlockedFrom.Right && speed > 0) {
				Debug.Log ("Blcked Right");
				return;
			}
			if (blockedFrom == BlockedFrom.Left && speed < 0)
			{
				Debug.Log("blocked left");
				return;
			}
			blockedFrom = BlockedFrom.None;

			GetComponent<Rigidbody> ().MovePosition (vec); 
			mCurrentDistance = tempDistance;
			mCurrentIdx = tempIndex;
		} 
	}
	private enum BlockedFrom{Right,Left, None};
	private BlockedFrom blockedFrom = BlockedFrom.None;
	public LayerMask whatIsGround; 
	public Transform frontCheck;
	public float frontCheckRadius;


	Quaternion GetSquad(int idxFirstPoint, float t)
	{
		Quaternion Q0 = mNodes[idxFirstPoint - 1].Rot;
		Quaternion Q1 = mNodes[idxFirstPoint].Rot;
		Quaternion Q2 = mNodes[idxFirstPoint + 1].Rot;
		Quaternion Q3 = mNodes[idxFirstPoint + 2].Rot;

		Quaternion T1 = MathUtils.GetSquadIntermediate(Q0, Q1, Q2);
		Quaternion T2 = MathUtils.GetSquadIntermediate(Q1, Q2, Q3);

		return MathUtils.GetQuatSquad(t, Q1, Q2, T1, T2);
	}



	public Vector3 GetHermiteInternal(int idxFirstPoint, float t)
	{
		float t2 = t * t;
		float t3 = t2 * t;

		Vector3 P0 = mNodes[idxFirstPoint - 1].Point;
		Vector3 P1 = mNodes[idxFirstPoint].Point;
		Vector3 P2 = mNodes[idxFirstPoint + 1].Point;
		Vector3 P3 = mNodes[idxFirstPoint + 2].Point;

		float tension = 0.5f;	// 0.5 equivale a catmull-rom

		Vector3 T1 = tension * (P2 - P0);
		Vector3 T2 = tension * (P3 - P1);

		float Blend1 = 2 * t3 - 3 * t2 + 1;
		float Blend2 = -2 * t3 + 3 * t2;
		float Blend3 = t3 - 2 * t2 + t;
		float Blend4 = t3 - t2;

		return Blend1 * P1 + Blend2 * P2 + Blend3 * T1 + Blend4 * T2;
	}


	public Vector3 GetHermiteAtTime(float timeParam)
	{
		if (timeParam >= mNodes[mNodes.Count - 2].Distance)
			return mNodes[mNodes.Count - 2].Point;

		int c;
		for (c = 1; c < mNodes.Count - 2; c++)
		{
			if (mNodes[c].Distance > timeParam)
				break;
		}

		int idx = c - 1;
		float param = (timeParam - mNodes[idx].Distance) / (mNodes[idx + 1].Distance - mNodes[idx].Distance);
		param = MathUtils.Ease(param, mNodes[idx].EaseIO.x, mNodes[idx].EaseIO.y);

		return GetHermiteInternal(idx, param);
	}
}