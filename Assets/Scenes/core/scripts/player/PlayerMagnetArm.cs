using System;
using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UnityEngine.Networking;

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
			LockedObject = 1 << 2,
			HeldObject = 1 << 3,
		}
		
		/// Inspector variables
		[SerializeField] private float mPullRate;
		[SerializeField] private float mPullRadius;
		[SerializeField] private LayerMask mGrabLayers;

		/// Private variables
		private WeaponPickupScript mHeldObject;
		private IAudioReference mGrabSound;
		private CltPlayer mBearer;
		private WeaponPickupScript mGrabCandidate;

		private const float SNAP_THRESHOLD_DISTANCE = 2.5f;
		private const float THROW_HOLD_SECONDS = 1.0f;

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
			}
			else
			{
				writer.Write((byte)syncVarDirtyBits);
				DirtyBitFlags flags = (DirtyBitFlags)syncVarDirtyBits;

				if ((flags & DirtyBitFlags.Bearer) != 0)
					writer.Write(bearer.netId);
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
			}
			else
			{
				DirtyBitFlags flags = (DirtyBitFlags)reader.ReadByte();

				if ((flags & DirtyBitFlags.Bearer) != 0)
					StartCoroutine(BindToBearer(reader.ReadNetworkId()));
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

		#endregion

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being held down.
		/// Result depends on our internal state.
		/// </summary>
		[Client]
		public void FireHeld()
		{
			UpdateSound(true);
		}

		/// <summary>
		/// INPUT_HANDLER: Handle the player's "magnet arm fire" button being released.
		/// Result depends on our internal state.
		/// </summary>
		[Client]
		public void FireUp()
		{
			UpdateSound(false);
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
				if (grabbable != null && !grabbable.currentlyLocked)
				{
					mGrabCandidate = grabbable;
					return;
				}
			}

			mGrabCandidate = null;
		}
	}
}
