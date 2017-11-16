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
		bool currentlyHeld { get; }

		/// <summary>
		/// Apply a physics force towards the provided pulling player.
		/// </summary>
		/// <param name="player">The player to pull towards. Must have a valid magnet arm.</param>
		void PullTowards(CltPlayer player);

		/// <summary>
		/// Snap from being in physics mode to being locked/"grabbed" in the player's hand.
		/// </summary>
		/// <param name="player">The player that is grabbing this part.</param>
		void GrabNow(CltPlayer player);

		/// <summary>
		/// Throw this part with physics force based on where our current holder is looking.
		/// </summary>
		void Throw();

		/// <summary>
		/// Release this part and let it fall to the ground without any extra force.
		/// </summary>
		void Release();
	}
}
