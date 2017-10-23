using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay
{
	public class PlayerMagnetArm : NetworkBehaviour
	{
		private enum State
		{
			Idle,
			Reeling,
			Locked
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

			// read if we have a held object
			if (reader.ReadBoolean())
			{
				GameObject heldObject = ClientScene.FindLocalObject(reader.ReadNetworkId());
				if (heldObject == null)
				{
					Logger.Warn("OnDeserialize: Magnet arm server is holding an object that does not exist on client!", Logger.System.Network);
					return;
				}

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

		[Client]
		private void UpdateSound(bool shouldPlay)
		{
			if (shouldPlay && mGrabSound == null)
			{
				mGrabSound = ServiceLocator.Get<IAudioManager>()
					.PlaySound(AudioManager.AudioEvent.LoopGravGun, mAudioProfile, transform);
			}
			else if (!shouldPlay && mGrabSound != null)
			{
				mGrabSound.Kill();
				mGrabSound = null;
			}
		}

		[Client]
		private void TryFindGrabCandidate()
		{
			RaycastHit hitInfo;

			Ray r = new Ray(bearer.eye.position, bearer.eye.forward);

			UnityEngine.Debug.DrawLine(r.origin, r.origin + r.direction * 1000.0f, Color.green, 0.1f, true);

			//if (!Physics.SphereCast(r, 0.6f, out hitInfo))
			if (!Physics.Raycast(r, out hitInfo))
				return;

			WeaponPickupScript grabbable = hitInfo.collider.GetComponentUpwards<WeaponPickupScript>();
			if (grabbable != null && !grabbable.currentlyHeld)
				mGrabCandidate = grabbable;
		}

		[Client]
		private void ReelGrabCandidate()
		{
			if (mGrabCandidate == null)
				return;

			mState = State.Reeling;

			Vector3 direction = mGrabCandidate.transform.position - bearer.eye.position;

			if (direction.magnitude < 2.5f)
			{
				CmdGrabItem(mGrabCandidate.netId);
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

		[Client]
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

		[Server]
		public void ForceDropItem()
		{
			if (mHeldObject == null)
				return;

			CmdReleaseItem(mHeldObject.netId, true);
			RpcForceReleaseItem();
		}

		[ClientRpc]
		private void RpcForceReleaseItem()
		{
			mState = State.Idle;
			ThrowOrDropItem();
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
			mGrabCandidate = go.GetComponent<WeaponPickupScript>();

			if (mGrabCandidate.currentlyHeld)
				return;

			mGrabCandidate.GrabNow(bearer);
			mHeldObject = mGrabCandidate;
			mGrabCandidate = null;
		}

		[Command]
		private void CmdReleaseItem(NetworkInstanceId itemId, bool drop)
		{
			mHeldObject = null;

			GameObject obj = NetworkServer.FindLocalObject(itemId);
			if (obj == null)
				return;

			WeaponPickupScript script = obj.GetComponent<WeaponPickupScript>();
			if (script == null)
				return;

			if (drop)
				script.Release();
			else
				script.Throw();
		}
	}
}
