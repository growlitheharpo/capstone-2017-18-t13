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

		private const float SNAP_THRESHOLD_DISTANCE = 2.5f;
		private const float THROW_HOLD_SECONDS = 1.0f;

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

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being held down.
		/// Result depends on our internal state.
		/// </summary>
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

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being released.
		/// Result depends on our internal state.
		/// </summary>
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

		/// <summary>
		/// Send an update to FMOD on whether or not to play the magnet arm "grab" sound.
		/// </summary>
		/// <param name="shouldPlay">Whether or not the sound should be playing.</param>
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

		/// <summary>
		/// Fire out a spherecast and check the objects it hit. Sets mGrabCandidate to null if
		/// no objects are found, or to the closeset hit object.
		/// </summary>
		[Client]
		private void TryFindGrabCandidate()
		{
			Ray r = new Ray(bearer.eye.position, bearer.eye.forward);

			UnityEngine.Debug.DrawLine(r.origin, r.origin + r.direction * 1000.0f, Color.green, 0.1f, true);

			var hits = Physics.SphereCastAll(r, mPullRadius, mGrabLayers);
			if (hits.Length == 0)
				return;

			// could also sort by dot product instead of distance to get the "most accurate" pull instead of the closest.
			hits = hits.OrderBy(x => x.distance).ToArray(); 

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

		/// <summary>
		/// Pull our current grab candidate towards us if we have one. Will abort if we are holding a part.
		/// When the object is closer than SNAP_THRESHOLD_DISTANCE, snaps the object into the hand.
		/// </summary>
		[Client]
		private void ReelGrabCandidate()
		{
			if (mGrabCandidate == null || heldWeaponPart != null)
				return;

			mState = State.Reeling;

			// Check if it's close enough to grab.
			Vector3 direction = mGrabCandidate.transform.position - bearer.eye.position;
			if (direction.magnitude < SNAP_THRESHOLD_DISTANCE)
			{
				EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.MagnetArmGrab, false));
				EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, true));
				CmdGrabItem(mGrabCandidate.netId);

				ServiceLocator.Get<IAudioManager>()
					.CreateSound(AudioEvent.MagnetArmGrab, transform)
					.AttachToRigidbody(mBearer.GetComponent<Rigidbody>());

				return;
			}

			// Check if we're still more or less looking at the object.
			Vector3 looking = bearer.eye.forward;
			float dot = Vector3.Dot(direction.normalized, looking.normalized);
			if (dot < 0.9f)
			{
				mGrabCandidate = null;
				return;
			}

			// Pull it towards us locally and on the server.
			mGrabCandidate.PullTowards(bearer);
			CmdReelObject(mGrabCandidate.netId);
		}

		/// <summary>
		/// Releases our currently held item. Throws or drops depending on the current hold time.
		/// </summary>
		[Client]
		private void ThrowOrDropItem()
		{
			EventManager.Notify(() => EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, false));
			if (heldWeaponPart != null && heldWeaponPart.currentHolder == bearer)
			{
				bool throwObj = mHeldTimer >= THROW_HOLD_SECONDS;

				if (throwObj)
					heldWeaponPart.Throw();
				else
					heldWeaponPart.Release();

				CmdReleaseItem(heldWeaponPart.netId, !throwObj);
			}

			heldWeaponPart = null;
			mGrabCandidate = null;
			mHeldTimer = 0.0f;
			mState = State.Idle;
		}

		/// <summary>
		/// Force the magnet arm to immediately drop a held item. Will not throw if there is no held item.
		/// </summary>
		[Server]
		public void ForceDropItem()
		{
			if (heldWeaponPart == null)
				return;

			CmdReleaseItem(heldWeaponPart.netId, true);
			RpcForceReleaseItem();
		}

		/// <summary>
		/// Reflect a "ForceDropItem" call on the server on each local instance.
		/// </summary>
		[ClientRpc]
		private void RpcForceReleaseItem()
		{
			mState = State.Idle;
			ThrowOrDropItem();
		}

		/// <summary>
		/// Reflect an item reel on the server so that all clients can sync it.
		/// </summary>
		/// <param name="id">The id of the object to reel in.</param>
		[Command]
		private void CmdReelObject(NetworkInstanceId id)
		{
			GameObject go = NetworkServer.FindLocalObject(id);
			go.GetComponent<INetworkGrabbable>().PullTowards(bearer);
		}

		/// <summary>
		/// Reflect an immediate grab/snap command from the client on the server so that all clients can sync it.
		/// </summary>
		/// <param name="id">The id of the object to snap.</param>
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

		/// <summary>
		/// Reflect a "release" command on the server so that all clients can sync it.
		/// </summary>
		/// <param name="itemId">The id of the object to release. Should be our held weapon.</param>
		/// <param name="drop">True to simply drop the object to the ground, false to throw it with force.</param>
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
