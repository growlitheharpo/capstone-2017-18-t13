﻿using System;
using System.Collections;
using FiringSquad.Core;
using FiringSquad.Debug;
using FiringSquad.Gameplay;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Prototyping
{
	public class SampleTargetScript : MonoBehaviour, IDamageReceiver
	{
		[SerializeField] private ParticleSystem mDeathParticles;
		[SerializeField] private GameObject mHitIndicator;
		[SerializeField] private GameObject mMesh;
		[SerializeField] private UIText mText;
		[SerializeField] private float mStartHealth;

		private BoundProperty<float> mHealth;
		public BoundProperty<float> health { get { return mHealth; } }

		private void Start()
		{
			mHealth = new BoundProperty<float>(mStartHealth, (gameObject.name + "-health").GetHashCode());
			ServiceLocator.Get<IGameConsole>()
				.RegisterCommand("target", CONSOLE_Reset);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IGameConsole>()
				.UnregisterCommand("target");

			mHealth.Cleanup();
		}

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

		private void Die()
		{
			mMesh.SetActive(false);
			mDeathParticles.Play();
		}
	}
}