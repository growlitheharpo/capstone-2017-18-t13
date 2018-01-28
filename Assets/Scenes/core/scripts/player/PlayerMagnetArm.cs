using System;
using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Gameplay.Weapons;
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
			Bearer = 1 << 1,
			HeldObject = 1 << 2,
		}

		/// Inspector variables
		[SerializeField] private float mClickHoldThreshold = 0.2f;
		[SerializeField] private float mPullRate;
		[SerializeField] private float mPullRadius;
		[SerializeField] private LayerMask mGrabLayers;

		/// Private variables
		private CltPlayer mBearer;
		private IAudioReference mGrabSound;
		private WeaponPickupScript mReelingObject;
		private float mInputTime;

		private const float SNAP_THRESHOLD_DISTANCE = 2.5f;
		private const float THROW_HOLD_SECONDS = 1.0f;

		/// <summary>
		/// Private reference at the object we're reeling. Setting this updates it on the network.
		/// </summary>
		private WeaponPickupScript reelingObject
		{
			get { return mReelingObject; }
			set
			{
				mReelingObject = value;
				SetDirtyBit(syncVarDirtyBits | (uint)DirtyBitFlags.HeldObject);
			}
		}

		/// <summary>
		/// Reference to the current weapon part that has been snapped into the player's hand.
		/// </summary>
		public WeaponPickupScript currentlyHeldObject
		{
			get
			{
				if (reelingObject != null && reelingObject.transform.parent == transform)
					return reelingObject;
				return null;
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

				if (reelingObject == null)
					writer.Write(false);
				else
				{
					writer.Write(true);
					writer.Write(reelingObject.netId);
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
					if (reelingObject == null)
						writer.Write(false);
					else
					{
						writer.Write(true);
						writer.Write(reelingObject.netId);
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
				DeserializeReelingObject(reader);
			}
			else
			{
				DirtyBitFlags flags = (DirtyBitFlags)reader.ReadByte();

				if ((flags & DirtyBitFlags.Bearer) != 0)
					StartCoroutine(BindToBearer(reader.ReadNetworkId()));
				if ((flags & DirtyBitFlags.HeldObject) != 0)
					DeserializeReelingObject(reader);
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
		/// Read and determine the object we are reeling based on the data in the stream.
		/// </summary>
		/// <param name="reader">The serialization stream, advanced to the position of the reeling object data.</param>
		private void DeserializeReelingObject(NetworkReader reader)
		{
			if (reader.ReadBoolean() == false) // false means there is no reeling object
			{
				if (mReelingObject != null)
				{
					mReelingObject.UnlockFromReel();
					mReelingObject = null;
				}
			}
			else // there is an object in the stream
			{
				GameObject obj = ClientScene.FindLocalObject(reader.ReadNetworkId());
				if (obj == null)
				{
					Logger.Warn("PlayerMagnetArm::DeserializeReelingObject could not find held object!");
					return;
				}

				WeaponPickupScript script = obj.GetComponent<WeaponPickupScript>();
				if (script == null)
				{
					Logger.Warn("PlayerMagnetArm::DeserializeReelingObject could not find weapon script on held object!");
					return;
				}

				mReelingObject = script;
				mReelingObject.LockToPlayerReel(mBearer);
			}
		}

		#endregion

		/// <summary>
		/// INPUT_HANDLER: Handle the player first pressing the "magnet arm fire" button.
		/// </summary>
		[Client]
		public void FirePressed()
		{
			// If we have an object in-hand, ignore the initial "press" event
			if (reelingObject != null)
				return;

			TryFindGrabCandidate();

			if (reelingObject == null)
			{
				// TODO: We need a sound event to play when the player tries to grab with nothing there.
				mInputTime = 0.0f;
			}
			else 
			{
				// start to grab this object
				reelingObject.LockToPlayerReel(bearer);
				CmdAssignClientAuthority(reelingObject.netId);
				mInputTime = float.NegativeInfinity;
			}
		}

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being held down.
		/// </summary>
		[Client]
		public void FireHeld()
		{
			if (reelingObject == null)
				return;

			if (reelingObject.transform.parent == transform)
			{
				// the object already snapped into place, we just need to tick that timer.
				mInputTime += Time.deltaTime;
				return;
			}

			// otherwise, we need to reel it in.
			reelingObject.TickReelToPlayer(mPullRate, mInputTime);
			if (Vector3.Distance(reelingObject.transform.position, transform.position) < SNAP_THRESHOLD_DISTANCE)
				reelingObject.SnapIntoReelPosition();
		}

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being released.
		/// </summary>
		[Client]
		public void FireUp()
		{
			if (mInputTime < 0.0f)
			{
				// if the input was less than zero seconds, we were in our "pull" button cycle.
				// Check if we successfully grabbed it. If not, let it go.
				if (reelingObject != null && reelingObject.transform.parent != transform)
				{
					reelingObject.UnlockFromReel();
					reelingObject = null;
				}
			}
			else if (mInputTime < mClickHoldThreshold)
			{
				// This counts as a press. Equip the item.
				bearer.CmdActivateInteract(bearer.eye.position, bearer.eye.forward);
			}
			else
			{
				// this counts as a throw. Throw it.
				if (reelingObject != null)
					CmdThrowHeldItem(bearer.eye.forward);
			}

			UpdateSound(false);
			mInputTime = 0.0f;
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
		/// Fire out a spherecast and check the objects it hit. Sets reelingObject to null if
		/// no objects are found, or to the closeset hit object.
		/// </summary>
		[Client]
		private void TryFindGrabCandidate()
		{
			Ray r = new Ray(bearer.eye.position, bearer.eye.forward);

			var hits = Physics.SphereCastAll(r, mPullRadius, mGrabLayers);
			if (hits.Length == 0)
				return;

			// could also sort by dot product instead of distance to get the "most accurate" pull instead of the closest.
			hits = hits.OrderBy(x => x.distance).ToArray(); 

			foreach (RaycastHit hitInfo in hits)
			{
				WeaponPickupScript grabbable = hitInfo.collider.GetComponentInParent<WeaponPickupScript>();
				if (grabbable != null && !grabbable.currentlyLocked)
				{
					reelingObject = grabbable;
					return;
				}
			}

			reelingObject = null;
		}

		/// <summary>
		/// Throw the item that we are currently holding on the server.
		/// Requires running on the server because 
		/// </summary>
		/// <param name="currentForward">The current forward vector of the player.</param>
		[Command]
		private void CmdThrowHeldItem(Vector3 currentForward)
		{
			if (reelingObject == null)
				return;

			reelingObject.UnlockAndThrow(currentForward * 30.0f);
			reelingObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(bearer.connectionToClient);
			reelingObject = null;
		}

		/// <summary>
		/// Give this client control over an item that it is trying to reel in.
		/// </summary>
		/// <param name="grabCandidateNetId">The network id of object the client is reeling.</param>
		[Command]
		private void CmdAssignClientAuthority(NetworkInstanceId grabCandidateNetId)
		{
			GameObject obj = NetworkServer.FindLocalObject(grabCandidateNetId);
			bool success = obj.GetComponent<NetworkIdentity>().AssignClientAuthority(bearer.connectionToClient);
			if (success)
				Logger.Warn("PlayerMagnetArm::AssignClientAuthority was unnsuccessful!");
		}

		/// <summary>
		/// Release this client's control over an item that it is trying to reel in.
		/// </summary>
		/// <param name="heldObjectId">The network id of object the client was reeling.</param>
		[Command]
		private void CmdReleaseClientAuthority(NetworkInstanceId heldObjectId)
		{
			GameObject obj = NetworkServer.FindLocalObject(heldObjectId);
			bool success = obj.GetComponent<NetworkIdentity>().RemoveClientAuthority(bearer.connectionToClient);
			if (success)
				Logger.Warn("PlayerMagnetArm::ReleaseClientAuthority was unnsuccessful!");
		}

		/// <summary>
		/// Immediately drop any item that this magnet arm is reeling or considering pulling.
		/// </summary>
		[Server]
		public void ForceDropItem()
		{
			if (reelingObject == null)
				return;

			reelingObject.UnlockFromReel();
			reelingObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(bearer.connectionToClient);
			reelingObject = null;
		}
	}
}
