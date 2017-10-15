using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class RemotePlayerHitIndicator : MonoBehaviour, IPlayerHitIndicator
	{
		private static GameObject kHitParticlesPrefab;

		private void Awake()
		{
			if (kHitParticlesPrefab == null)
				kHitParticlesPrefab = Resources.Load<GameObject>("prefabs/player/p_hitParticles");
		}

		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			GameObject instance = Instantiate(kHitParticlesPrefab, hitPosition, Quaternion.LookRotation(hitNormal, transform.up));
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(instance.GetComponent<ParticleSystem>()));
		}
	}
}
