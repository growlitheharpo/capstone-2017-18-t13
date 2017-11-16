using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// A hit indicator that does absolutely nothing, to avoid null reference exceptions.
	/// </summary>
	public class NullHitIndicator : IPlayerHitIndicator
	{
		/// <inheritdoc />
		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			// Do nothing
		}
	}
}
