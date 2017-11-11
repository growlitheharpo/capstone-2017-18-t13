using System;
using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// The networked magnet arm of the player.
	/// Used to draw weapon parts towards us and hold onto them until the player
	/// decides to equip, drop, or throw them.
	/// </summary>
	public class PlayerMagnetArm : NetworkBehaviour
	{
		/// <summary> Enum used to mark network data that needs to be updated. </summary>
		[Flags]
		private enum DirtyBitFlags
		{
			Bearer = 0x1,
			HeldObject = 0x2,
		}

		/// <summary> The current input state of the magnet arm. </summary>
		private enum State
		{
			Idle,
			Reeling,
			Locked
		}

		/// Inspector variables
		[SerializeField] private float mPullForce;
		[SerializeField] private float mPullRadius;
		[SerializeField] private LayerMask mGrabLayers;

		/// Private variables
		private WeaponPickupScript mHeldObject;
		private IAudioReference mGrabSound;
		private CltPlayer mBearer;
		private WeaponPickupScript mGrabCandidate;
		private float mHeldTimer;
		private State mState;

		/// <summary> 
		/// The pull force of this magnet arm. 
		/// TODO: Does this need to be public or can it be passed as a parameter where it is used?
		/// </summary>
		public float pullForce { get { return mPullForce; } }

		/// <summary>
		/// The current held weapon part of this magnet arm, or null if we are not holding one.
		/// </summary>
		[CanBeNull] public WeaponPickupScript heldWeaponPart
		{
			get { return mHeldObject; }
			set
			{
				if (mHeldObject == value)
					return;

				mHeldObject = value;
				SetDirtyBit(syncVarDirtyBits | (uint)DirtyBitFlags.HeldObject);
			}
		}

		/// <summary>
		/// The current bearer of this magnet arm.
		/// </summary>
		public CltPlayer bearer
		{
			get { return mBearer; }
			set
			{
				mBearer = value;
				SetDirtyBit(syncVarDirtyBits | (uint)DirtyBitFlags.Bearer);
			}
		}

		#region Serialization

		/// <summary>
		/// Unity's OnSerialize function. Called when any dirty bits have been set.
		/// </summary>
		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write(bearer.netId);

				if (heldWeaponPart == null)
					writer.Write(false);
				else
				{
					writer.Write(true);
					writer.Write(heldWeaponPart.netId);
				}
			}
			else
			{
				writer.Write((byte)syncVarDirtyBits);
				DirtyBitFlags flags = (DirtyBitFlags)syncVarDirtyBits;

				if ((flags & DirtyBitFlags.Bearer) != 0)
					writer.Write(bearer.netId);
				if ((flags & DirtyBitFlags.HeldObject) != 0)
				{
					if (heldWeaponPart == null)
						writer.Write(false);
					else
					{
						writer.Write(true);
						writer.Write(heldWeaponPart.netId);
					}
				}
			}

			ClearAllDirtyBits();
			return true;
		}

		/// <summary>
		/// Unity's OnSerialize function. Called when an update to our state has been received.
		/// </summary>
		public override void OnDeserialize(NetworkReader reader, bool forceAll)
		{
			if (forceAll)
			{
				NetworkInstanceId id = reader.ReadNetworkId();
				StartCoroutine(BindToBearer(id));

				DeserializeHeldObject(reader);
			}
			else
			{
				DirtyBitFlags flags = (DirtyBitFlags)reader.ReadByte();

				if ((flags & DirtyBitFlags.Bearer) != 0)
					StartCoroutine(BindToBearer(reader.ReadNetworkId()));
				if ((flags & DirtyBitFlags.HeldObject) != 0)
					DeserializeHeldObject(reader);
			}
		}

		/// <summary>
		/// Determine our held object from the network reader.
		/// </summary>
		private void DeserializeHeldObject(NetworkReader reader)
		{
			// Check if we have a part. If not, make sure to locally reflect that.
			bool hasPart = reader.ReadBoolean();
			if (!hasPart)
			{
				if (mHeldObject != null)
					mHeldObject.Release();
				mHeldObject = null;
			}
			else
			{
				// We do have a part. Try to find it and grab it.
				GameObject heldObject = ClientScene.FindLocalObject(reader.ReadNetworkId());
				if (heldObject == null)
				{
					Logger.Warn("OnDeserialize: Magnet arm server is holding an object that does not exist on client!", Logger.System.Network);
					return;
				}

				mHeldObject = heldObject.GetComponent<WeaponPickupScript>();
				if (mHeldObject == null)
				{
					Logger.Warn("OnDeserialize: Magnet arm server is holding an object that does not exist on client!", Logger.System.Network);
					return;
				}

				if (mHeldObject.currentHolder != bearer)
					mHeldObject.GrabNow(bearer);
			}
		}

		/// <summary>
		/// Bind this magnet arm to the player with the provided ID.
		/// </summary>
		private IEnumerator BindToBearer(NetworkInstanceId bearerId)
		{
			while (bearer == null || bearer.netId != bearerId)
			{
				GameObject obj = ClientScene.FindLocalObject(bearerId);
				if (obj != null)
					bearer = obj.GetComponent<CltPlayer>();

				if (bearer != null)
				{
					bearer.BindMagnetArmToPlayer(this);
					yield break;
				}

				yield return null;
			}
		}

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			if (bearer == null || !bearer.isCurrentPlayer)
				return;

			// TODO: This is *not* how these next few lines should work. We should only send the events when something has changed!
			TryFindGrabCandidate();
			EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.MagnetArmGrab, mGrabCandidate != null));
			EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, heldWeaponPart != null));
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
					mState = heldWeaponPart != null ? State.Locked : State.Idle;
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
					.CreateSound(AudioEvent.LoopGravGun, transform).AttachToRigidbody(mBearer.GetComponent<Rigidbody>());
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
			Ray r = new Ray(bearer.eye.position, bearer.eye.forward);

			UnityEngine.Debug.DrawLine(r.origin, r.origin + r.direction * 1000.0f, Color.green, 0.1f, true);

			var hits = Physics.SphereCastAll(r, mPullRadius, mGrabLayers);
			if (hits.Length == 0)
				return;

			hits = hits.OrderBy(x => x.distance).ToArray(); // could also sort by dot product

			foreach (RaycastHit hitInfo in hits)
			{
				WeaponPickupScript grabbable = hitInfo.collider.GetComponentInParent<WeaponPickupScript>();
				if (grabbable != null && !grabbable.currentlyHeld)
				{
					mGrabCandidate = grabbable;
					return;
				}
			}

			mGrabCandidate = null;
		}

		[Client]
		private void ReelGrabCandidate()
		{
			if (mGrabCandidate == null || heldWeaponPart != null)
				return;

			mState = State.Reeling;

			Vector3 direction = mGrabCandidate.transform.position - bearer.eye.position;

			if (direction.magnitude < 2.5f)
			{
				EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.MagnetArmGrab, false));
				EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, true));
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
			EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, false));
			if (heldWeaponPart != null && heldWeaponPart.currentHolder == bearer)
			{
				if (mHeldTimer >= 1.0f)
					heldWeaponPart.Throw();
				else
					heldWeaponPart.Release();

				CmdReleaseItem(heldWeaponPart.netId, mHeldTimer < 1.0f);
			}

			heldWeaponPart = null;
			mGrabCandidate = null;
			mHeldTimer = 0.0f;
			mState = State.Idle;
		}

		[Server]
		public void ForceDropItem()
		{
			if (heldWeaponPart == null)
				return;

			CmdReleaseItem(heldWeaponPart.netId, true);
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
			heldWeaponPart = mGrabCandidate;
			mGrabCandidate = null;
		}

		[Command]
		private void CmdReleaseItem(NetworkInstanceId itemId, bool drop)
		{
			heldWeaponPart = null;

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
