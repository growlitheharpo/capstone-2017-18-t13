using System;
using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UnityEngine.Assertions;
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
		[SerializeField] private float mPullRate;
		[SerializeField] private float mPullRadius;
		[SerializeField] private LayerMask mGrabLayers;

		/// Private variables
		private CltPlayer mBearer;
		private IAudioReference mGrabSound;
		private WeaponPickupScript mReelingObject;
		private float mReelingTime;
		private bool mPushedCrosshairHint;
		private Animator mViewAnimator;

		private const float SNAP_THRESHOLD_DISTANCE = 2.5f;

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
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			Transform view = transform.Find("View");
			mViewAnimator = view.GetChild(0).GetComponent<Animator>();
		}

		/// <summary>
		/// Public callback for the magnet arm after it has been bound to its owner, client-side or server-side.
		/// </summary>
		/// <param name="forceLocalPlayer">Force this magnet arm to act like the local player.</param>
		public void OnPostBind(bool forceLocalPlayer = false)
		{
			Assert.IsTrue(bearer != null, "PlayerMagnetArm.OnPostBind called but bearer is null!");
			if (bearer.isCurrentPlayer || forceLocalPlayer)
				return;

			// If our bearer is NOT the current player, destroy our view.
			transform.Find("View").gameObject.SetActive(false);
		}

		/// <summary>
		/// Force the view to be visible on the server.
		/// </summary>
		public void SetViewVisible()
		{
			transform.Find("View").gameObject.SetActive(true);
		}

		/// <summary>
		/// Unity's Update function
		/// </summary>
		[ClientCallback]
		private void Update()
		{
			if (bearer != null && bearer.isCurrentPlayer)
				UpdateCrosshairHints();

			if (mViewAnimator != null && mViewAnimator.isActiveAndEnabled)
				mViewAnimator.SetBool("PartInHand", reelingObject != null);
		}

		/// <summary>
		/// Ensure that the crosshair hint is up-to-date.
		/// </summary>
		private void UpdateCrosshairHints()
		{
			if (reelingObject != null)
				return;

			WeaponPickupScript potentialTarget = TryFindGrabCandidate();
			if (potentialTarget != null && !mPushedCrosshairHint)
			{
				EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.MagnetArmGrab, true);
				mPushedCrosshairHint = true;
			}
			else if (potentialTarget == null && mPushedCrosshairHint)
			{
				EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.MagnetArmGrab, false);
				mPushedCrosshairHint = false;
			}
		}

		/// <summary>
		/// INPUT_HANDLER: Handle the player first pressing the "magnet arm fire" button.
		/// </summary>
		[Client]
		public void FirePressed()
		{
			if (!bearer.isCurrentPlayer)
				return;

			// If we have an object in-hand, ignore the initial "press" event
			if (reelingObject != null)
				return;

			reelingObject = TryFindGrabCandidate();

			if (reelingObject != null)
			{
				// start to grab this object
				reelingObject.LockToPlayerReel(bearer);
				CmdAssignClientAuthority(reelingObject.netId);
				mReelingTime = 0.0f;
				UpdateReelingSound(true);
			}
			else 
			{
				// TODO: We need to play a sound when there is no object available
			}
		}

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being held down.
		/// </summary>
		[Client]
		public void FireHeld()
		{
			if (!bearer.isCurrentPlayer)
				return;

			if (reelingObject == null)
				return;

			if (reelingObject.transform.parent == transform)
			{
				// the object already snapped into place
				return;
			}

			// otherwise, we need to reel it in.
			mReelingTime += Time.deltaTime;
			reelingObject.TickReelToPlayer(mPullRate, mReelingTime);
			if (Vector3.Distance(reelingObject.transform.position, transform.position) < SNAP_THRESHOLD_DISTANCE)
			{
				if (mPushedCrosshairHint)
					EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.MagnetArmGrab, false);
				
				EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, true);
				mPushedCrosshairHint = true;

				reelingObject.SnapIntoReelPosition();
				UpdateReelingSound(false);
				PlaySnappedSound();
			}
		}

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being released.
		/// </summary>
		[Client]
		public void FireUp()
		{
			if (!bearer.isCurrentPlayer)
				return;

			if (mReelingTime > 0.0f)
			{
				// if the input was less than zero seconds, we were in our "pull" button cycle.
				// Check if we successfully grabbed it. If not, let it go.
				if (reelingObject != null && reelingObject.transform.parent != transform)
				{
					reelingObject.UnlockFromReel();
					reelingObject = null;
				}
			}
			else
			{
				// This counts as a press. Equip the item.
				if (reelingObject != null)
					bearer.CmdActivateInteractWithObject(reelingObject.netId);
				else
					bearer.CmdActivateInteract(bearer.eye.position, bearer.eye.forward);

				// Hide the "press" hint
				EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, false);
				mPushedCrosshairHint = false;
			}

			UpdateReelingSound(false);
			mReelingTime = -1.0f;
		}

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm drop" button being released.
		/// </summary>
		[Client]
		public void DropItemDown()
		{
			// The player hit the throw button. Throw it.
			if (reelingObject != null)
			{
				reelingObject.UnlockAndThrow(bearer.eye.forward * 30.0f);
				CmdThrowHeldItem(bearer.eye.forward, reelingObject.netId);
				reelingObject = null;

				// Hide the "throw" hint
				EventManager.LocalGUI.SetHintState(CrosshairHintText.Hint.ItemEquipOrDrop, false);
				mPushedCrosshairHint = false;
			}
		}

		/// <summary>
		/// Send an update to FMOD on whether or not to play the magnet arm "grab" sound.
		/// </summary>
		/// <param name="shouldPlay">Whether or not the sound should be playing.</param>
		[Client]
		private void UpdateReelingSound(bool shouldPlay)
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
		/// Immediately fire the "part snapped into hand" sound event.
		/// </summary>
		private void PlaySnappedSound()
		{
			ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.MagnetArmGrab, transform).AttachToRigidbody(bearer.GetComponent<Rigidbody>());
		}

		/// <summary>
		/// Fire out a spherecast and check the objects it hit. Sets reelingObject to null if
		/// no objects are found, or to the closeset hit object.
		/// </summary>
		[Client]
		private WeaponPickupScript TryFindGrabCandidate()
		{
			Ray r = new Ray(bearer.eye.position, bearer.eye.forward);

			var hits = Physics.SphereCastAll(r, mPullRadius, mGrabLayers);
			if (hits.Length == 0)
				return null;

			// could also sort by dot product instead of distance to get the "most accurate" pull instead of the closest.
			hits = hits.OrderBy(x => x.distance).ToArray(); 

			foreach (RaycastHit hitInfo in hits)
			{
				WeaponPickupScript grabbable = hitInfo.collider.GetComponentInParent<WeaponPickupScript>();
				if (grabbable != null && !grabbable.currentlyLocked)
					return grabbable;
			}

			return null;
		}

		/// <summary>
		/// Throw the item that we are currently holding on the server.
		/// Requires running on the server because 
		/// </summary>
		/// <param name="currentForward">The current forward vector of the player.</param>
		/// <param name="objectId">The network instance ID of the currently held object.</param>
		[Command]
		private void CmdThrowHeldItem(Vector3 currentForward, NetworkInstanceId objectId)
		{
			GameObject go = NetworkServer.FindLocalObject(objectId);
			if (go == null)
				return;

			WeaponPickupScript grabbable = go.GetComponent<WeaponPickupScript>();
			if (grabbable == null)
				return;

			bool success = grabbable.GetComponent<NetworkIdentity>().RemoveClientAuthority(bearer.connectionToClient);
			if (!success)
				Logger.Warn("PlayerMagnetArm::ReleaseClientAuthority was unnsuccessful!");

			grabbable.UnlockAndThrow(currentForward * 30.0f);

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
			if (!success)
				Logger.Warn("PlayerMagnetArm::AssignClientAuthority was unnsuccessful!");
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
