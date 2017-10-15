using System;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMagnetArm : NetworkBehaviour
{
	private enum State
	{
		Idle,
		Reeling,
		Locked,
	}

	[SerializeField] private float mPullForce;
	public float pullForce { get { return mPullForce; } }

	private INetworkGrabbable mHeldObject;
	public CltPlayer bearer { get; set; }

	private INetworkGrabbable mGrabCandidate;
	private float mHeldTimer;
	private State mState;

	#region Serialization

	// Todo: Optimize these to only send changes
	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		writer.Write(bearer.netId);

		if (mHeldObject == null)
			writer.Write(false);
		else
		{
			writer.Write(true);
			writer.Write(mHeldObject.netId);
		}

		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		// read our bearer
		NetworkInstanceId bearerId = reader.ReadNetworkId();
		if (bearer == null || bearer.netId != bearerId)
		{
			GameObject bearerObj = ClientScene.FindLocalObject(bearerId);
			if (bearerObj != null)
				bearerObj.GetComponent<CltPlayer>().BindMagnetArmToPlayer(this);
		}

		// read if we have a held object
		if (reader.ReadBoolean())
		{
			GameObject heldObject = ClientScene.FindLocalObject(reader.ReadNetworkId());
			if (heldObject == null)
				return;

			INetworkGrabbable grabbable = heldObject.GetComponent<INetworkGrabbable>();
			if (grabbable.currentHolder != bearer)
				grabbable.GrabNow(bearer);
		}
	}

	#endregion
	
	[Client]
	public void FireHeld()
	{
		switch (mState)
		{
			case State.Idle:
				TryReelObject();
				break;
			case State.Reeling:
				TryReelObject();
				break;
			case State.Locked:
				mHeldTimer += Time.deltaTime;
				break;
		}
	}

	[Client]
	public void FireUp()
	{
		switch (mState)
		{
			case State.Idle:
				break;
			case State.Reeling:
				mState = mHeldObject != null ? State.Locked : State.Idle;
				break;
			case State.Locked:
				ThrowOrDropItem();
				break;
		}
	}

	private void TryReelObject()
	{
		INetworkGrabbable grabbable;
		RaycastHit hitInfo;

		Ray r = new Ray(bearer.eye.position, bearer.eye.forward);

		Debug.DrawLine(r.origin, r.origin + r.direction * 1000.0f, Color.green, 0.1f, true);

		//if (!Physics.SphereCast(r, 0.6f, out hitInfo))
		if (!Physics.Raycast(r, out hitInfo))
			return;

		grabbable = hitInfo.collider.GetComponentUpwards<INetworkGrabbable>();
		if (grabbable != null && !grabbable.currentlyHeld)
			ReelObject(grabbable);
	}

	private void ReelObject(INetworkGrabbable go)
	{
		go.PullTowards(bearer);
		CmdReelObject(go.netId);
	}

	[Command]
	private void CmdReelObject(NetworkInstanceId obj)
	{
		GameObject go = NetworkServer.FindLocalObject(obj);
		go.GetComponent<INetworkGrabbable>().PullTowards(bearer);
	}

	private void ThrowOrDropItem()
	{
		
	}
}
