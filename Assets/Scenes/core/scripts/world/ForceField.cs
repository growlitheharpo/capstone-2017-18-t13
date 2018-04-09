using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class ForceField : NetworkBehaviour, IDamageReceiver
	{
		// Forcefield effect prefab to spawn
		[SerializeField] private GameObject mForcefieldEffectPrefab;

		/// <inheritdoc />
		public float currentHealth { get { return default(float); } }

		/// <summary>
		/// Doesn't actually deal damage, but spawns a prefab at the spot hit
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="point"></param>
		/// <param name="normal"></param>
		/// <param name="cause"></param>
		/// <param name="wasHeadshot"></param>
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause, bool wasHeadshot)
		{
			// Spawn a force field prefab at the point of hit
			RpcDisplayEffect(point, normal);
		}

		/// <inheritdoc /> Here because it has to be
		public void HealDamage(float amount) { }

		/// <summary>
		/// Display and destroy the assigned particle system.
		/// </summary>
		[ClientRpc]
		private void RpcDisplayEffect(Vector3 point, Vector3 normal)
		{
			GameObject tmp = Instantiate(mForcefieldEffectPrefab, point, transform.rotation);
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(tmp.GetComponent<ParticleSystem>()));
		}
	}
}
