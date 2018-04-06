using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class ForceField : MonoBehaviour, IDamageReceiver
	{
		private float mHealth;
		private GameObject mGameObject;

		// Forcefield effect prefab to spawn
		[SerializeField] GameObject mForcefieldEffectPrefab;

		/// <inheritdoc />
		public GameObject gameObject { get { return mGameObject; } }

		private IEnumerator mCoroutine;

		/// <inheritdoc /> Here because it has to be
		public float currentHealth { get { return mHealth; } }

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

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
			GameObject tmp = GameObject.Instantiate(mForcefieldEffectPrefab, point, transform.rotation);

			mCoroutine = WaitAndDestroy(tmp);
			StartCoroutine(mCoroutine);
		}

		/// <inheritdoc /> Here because it has to be
		public void HealDamage(float amount)
		{
		}

		/// <summary>
		/// Coroutine to destroy the particle system when it is done playing
		/// </summary>
		/// <param name="particlePrefab"></param>
		/// <returns></returns>
		IEnumerator WaitAndDestroy(GameObject particlePrefab)
		{
			// Wait for the duration of the particle
			yield return new WaitForSeconds(particlePrefab.GetComponent<ParticleSystem>().main.duration);

			// Destroy after the time
			Destroy(particlePrefab);
		}
	}
}



