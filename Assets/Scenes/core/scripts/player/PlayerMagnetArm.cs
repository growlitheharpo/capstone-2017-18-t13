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

	private WeaponPickupScript mHeldObject;
	public WeaponPickupScript heldWeaponPart { get { return mHeldObject; } }

	[SerializeField] private AudioProfile mAudioProfile;
	private IAudioReference mGrabSound;

	public CltPlayer bearer { get; set; }

	private WeaponPickupScript mGrabCandidate;
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

		if (bearer == null)
			return;

		if (bearer.isCurrentPlayer) // WE have authority if our bearer is the local player. No deserializing.
			return;

		// read if we have a held object
		if (reader.ReadBoolean())
		{
			GameObject heldObject = ClientScene.FindLocalObject(reader.ReadNetworkId());
			if (heldObject == null)
				return;

			mHeldObject = heldObject.GetComponent<WeaponPickupScript>();
			if (mHeldObject.currentHolder != bearer)
				mHeldObject.GrabNow(bearer);
		}
		else
		{
			if (mHeldObject != null)
				mHeldObject.Release();
			mHeldObject = null;
		}
	}

	private void Update()
	{
		SetDirtyBit(99999);
	}

	#endregion

	[Client]
	public void FireHeld()
	{
		switch (mState)
		{
			case State.Idle:
				if (mGrabCandidate == null)
					TryFindGrabCandidate();
				ReelGrabCandidate();
				UpdateSound(true);
				break;
			case State.Reeling:
				ReelGrabCandidate();
				UpdateSound(true);
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
				UpdateSound(false);
				break;
			case State.Reeling:
				mState = mHeldObject != null ? State.Locked : State.Idle;
				UpdateSound(false);
				break;
			case State.Locked:
				ThrowOrDropItem();
				break;
		}
	}

	private void UpdateSound(bool shouldPlay)
	{
		if (shouldPlay && mGrabSound == null)
		{
			ServiceLocator.Get<IAudioManager>()
				.PlaySound(AudioManager.AudioEvent.LoopGravGun, mAudioProfile, transform);
		}
		else if (!shouldPlay && mGrabSound != null)
		{
			mGrabSound.Kill();
			mGrabSound = null;
		}
	}

	private void TryFindGrabCandidate()
	{
		RaycastHit hitInfo;

		Ray r = new Ray(bearer.eye.position, bearer.eye.forward);

		Debug.DrawLine(r.origin, r.origin + r.direction * 1000.0f, Color.green, 0.1f, true);

		//if (!Physics.SphereCast(r, 0.6f, out hitInfo))
		if (!Physics.Raycast(r, out hitInfo))
			return;

		WeaponPickupScript grabbable = hitInfo.collider.GetComponentUpwards<WeaponPickupScript>();
		if (grabbable != null && !grabbable.currentlyHeld)
			mGrabCandidate = grabbable;
	}

	private void ReelGrabCandidate()
	{
		if (mGrabCandidate == null)
			return;

		mState = State.Reeling;

		Vector3 direction = mGrabCandidate.transform.position - bearer.eye.position;

		if (direction.magnitude < 2.5f)
		{
			GrabItem();
			return;
		}

		Vector3 looking = bearer.eye.forward;
		float dot = Vector3.Dot(direction.normalized, looking.normalized);
		if (dot < 0.9f)
		{
			mGrabCandidate = null;
			return;
		}

		mGrabCandidate.PullTowards(bearer);
		CmdReelObject(mGrabCandidate.netId);
	}

	private void GrabItem()
	{
		if (mGrabCandidate.currentlyHeld)
			return;

		mGrabCandidate.GrabNow(bearer);
		mHeldObject = mGrabCandidate;
		mGrabCandidate = null;

		CmdGrabItem(mHeldObject.netId);
	}

	private void ThrowOrDropItem()
	{
		if (mHeldObject != null && mHeldObject.currentHolder == bearer)
		{
			if (mHeldTimer >= 1.0f)
				mHeldObject.Throw();
			else
				mHeldObject.Release();

			CmdReleaseItem(mHeldObject.netId, mHeldTimer < 1.0f);
		}

		mHeldObject = null;
		mGrabCandidate = null;
		mHeldTimer = 0.0f;
		mState = State.Idle;
	}

	[Command]
	private void CmdReelObject(NetworkInstanceId id)
	{
		GameObject go = NetworkServer.FindLocalObject(id);
		go.GetComponent<INetworkGrabbable>().PullTowards(bearer);
	}

	[Command]
	private void CmdGrabItem(NetworkInstanceId id)
	{
		GameObject go = NetworkServer.FindLocalObject(id);
		WeaponPickupScript script = go.GetComponent<WeaponPickupScript>();

		script.GrabNow(bearer);
		mHeldObject = script;
	}

	[Command]
	private void CmdReleaseItem(NetworkInstanceId itemId, bool drop)
	{
		WeaponPickupScript go = NetworkServer.FindLocalObject(itemId).GetComponent<WeaponPickupScript>();

		if (drop)
			go.Release();
		else
			go.Throw();

		mHeldObject = null;
	}

	[ClientRpc]
	private void RpcReleaseItem(NetworkInstanceId itemId, bool drop)
	{
		
	}
}
