using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Interface for an object 
	/// </summary>
	public interface INetworkGrabbable
	{
		/// <summary>
		/// The netId of this object.
		/// </summary>
		NetworkInstanceId netId { get; }

		/// <summary>
		/// The transform of this object.
		/// </summary>
		Transform transform { get; }

		/// <summary>
		/// The gameobject of this object.
		/// </summary>
		GameObject gameObject { get; }

		/// <summary>
		/// The player who is currently holding us.
		/// </summary>
		[CanBeNull] CltPlayer currentHolder { get; }

		/// <summary>
		/// Whether or not this object is currently held.
		/// </summary>
		bool currentlyLocked { get; }

		/// <summary>
		/// Lock the grabbable to a player. Will not be grabbable by anyone else until unlocked.
		/// </summary>
		/// <param name="player"></param>
		void LockToPlayerReel(CltPlayer player);

		/// <summary>
		/// Unlock this grabbable from its current player. Will now be grabbable by anyone.
		/// </summary>
		void UnlockFromReel();

		/// <summary>
		/// Ticks the lerp towards a player by the given rate.
		/// </summary>
		/// <param name="pullRate">Units per second that we should move towards the player.</param>
		/// <param name="elapsedTime">The amount of time that has elapsed on this pull. Used for smoothing.</param>
		void TickReelToPlayer(float pullRate, float elapsedTime);

		/// <summary>
		/// Immediately snap the grabbable into the current user's hand.
		/// </summary>
		void SnapIntoReelPosition();

		/// <summary>
		/// Unlock this grabbable from its current player. Will now be grabbable by anyone.
		/// Also adds an immediate force to the grabbable.
		/// </summary>
		/// <param name="throwForce">The force to apply to the object.</param>
		void UnlockAndThrow(Vector3 throwForce);
	}
}
