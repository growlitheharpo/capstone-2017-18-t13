using System;
using System.Collections;
using FiringSquad.Core;
using FiringSquad.Debug;
using FiringSquad.Gameplay;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Prototyping
{
	/// <summary>
	/// Sample target. Not networked. Just receives and displays damage.
	/// </summary>
	public class SampleTargetScript : MonoBehaviour, IDamageReceiver
	{
		/// Inspector variables
		[SerializeField] private ParticleSystem mDeathParticles;
		[SerializeField] private GameObject mHitIndicator;
		[SerializeField] private GameObject mMesh;
		[SerializeField] private UIText mText;
		[SerializeField] private float mStartHealth;

		/// Private variables
		private BoundProperty<float> mHealth;

		/// <inheritdoc />
		public float currentHealth { get { return mHealth.value; } }

		/// <summary>
		/// The health of this target.
		/// </summary>
		public BoundProperty<float> health { get { return mHealth; } }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Start()
		{
			mHealth = new BoundProperty<float>(mStartHealth, (gameObject.name + "-health").GetHashCode());
			ServiceLocator.Get<IGameConsole>()
				.RegisterCommand("target", CONSOLE_Reset);
		}

		/// <summary>
		/// Cleanup listeners and event listeners.
		/// </summary>
		private void OnDestroy()
		{
			ServiceLocator.Get<IGameConsole>()
				.UnregisterCommand("target");

			mHealth.Cleanup();
		}

		/// <summary>
		/// Handle the console reset command.
		/// </summary>
		private static void CONSOLE_Reset(string[] args)
		{
			var allObjects = FindObjectsOfType<SampleTargetScript>();

			switch (args[0].ToLower())
			{
				case "reset":
					foreach (SampleTargetScript obj in allObjects)
					{
						obj.mHealth.value = obj.mStartHealth;
						obj.mMesh.SetActive(true);
					}
					break;
				case "sethealth":
					foreach (SampleTargetScript obj in allObjects)
					{
						obj.mHealth.value = float.Parse(args[1]);
						obj.mMesh.SetActive(true);
					}
					break;
				default:
					throw new ArgumentException("Invalid arguments for command: target");
			}
		}

		/// <inheritdoc />
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause = null)
		{
			StopAllCoroutines();
			mHealth.value = Mathf.Clamp(mHealth.value - amount, 0.0f, float.MaxValue);

			if (mHealth.value <= 0.0f)
				Die();

			mText.color = new Color(0.4f, 0.4f, 0.4f, 1.0f);
			mText.text = "Damage:\n" + amount.ToString("####");

			Instantiate(mHitIndicator, point, Quaternion.identity);

			StartCoroutine(FadeText());
		}

		/// <inheritdoc />
		public void HealDamage(float amount)
		{
			mHealth.value += amount;
		}

		/// <summary>
		/// Fade out our text after the hit occurs.
		/// </summary>
		private IEnumerator FadeText()
		{
			Color startCol = mText.color;
			float currentTime = 0.0f;

			while (currentTime < 0.75f)
			{
				mText.color = Color.Lerp(startCol, new Color(startCol.r, startCol.g, startCol.b, 0.0f), currentTime / 0.75f);
				currentTime += Time.deltaTime;
				yield return null;
			}

			yield return null;
		}

		/// <summary>
		/// Handle the target hitting 0 health.
		/// </summary>
		private void Die()
		{
			mMesh.SetActive(false);
			mDeathParticles.Play();
		}
	}
}
