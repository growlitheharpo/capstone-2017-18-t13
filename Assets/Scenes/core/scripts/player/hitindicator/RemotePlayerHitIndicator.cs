using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// The hit indicator for the non-local players in the scene.
	/// </summary>
	/// <inheritdoc cref="IPlayerHitIndicator" />
	public class RemotePlayerHitIndicator : MonoBehaviour, IPlayerHitIndicator
	{
		private static GameObject kHitParticlesPrefab;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			if (kHitParticlesPrefab == null)
				kHitParticlesPrefab = Resources.Load<GameObject>("prefabs/player/p_hitParticles");
		}

		/// <inheritdoc />
		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			GameObject instance = Instantiate(kHitParticlesPrefab, hitPosition, Quaternion.LookRotation(hitNormal, transform.up));
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(instance.GetComponent<ParticleSystem>()));
		}
	}
}
