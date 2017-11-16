using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Object that visualizes damage being done.
	/// </summary>
	public interface IPlayerHitIndicator
	{
		/// <summary>
		/// Notify this visualizer to show a hit has occurred.
		/// </summary>
		/// <param name="receiver">The receiver of the damage.</param>
		/// <param name="sourcePosition">The world postion where the shot originated from.</param>
		/// <param name="hitPosition">The world position where the hit took place.</param>
		/// <param name="hitNormal">The normal of the hit.</param>
		/// <param name="amount">How much damage was caused by this hit.</param>
		void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount);
	}
}
