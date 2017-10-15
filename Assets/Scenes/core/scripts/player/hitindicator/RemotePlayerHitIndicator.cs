using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class RemotePlayerHitIndicator : MonoBehaviour, IPlayerHitIndicator
	{
		private static GameObject mHitParticlesPrefab;

		private void Awake()
		{
			if (mHitParticlesPrefab == null)
				mHitParticlesPrefab = Resources.Load<GameObject>("prefabs/player/p_hitParticles");
		}

		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			
		}
	}
}
