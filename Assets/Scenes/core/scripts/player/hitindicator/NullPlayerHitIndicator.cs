using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class NullHitIndicator : IPlayerHitIndicator
	{
		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			// Do nothing
		}
	}
}

