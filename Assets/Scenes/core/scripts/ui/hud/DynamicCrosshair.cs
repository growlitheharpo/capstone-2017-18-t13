﻿using System.Collections;
using System.Linq;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that manages scaling the crosshair based on current dispersion.
	/// </summary>
	public class DynamicCrosshair : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private RectTransform mImageHolder;
		[SerializeField] private float mFadeTime;

		/// Private variables
		private CltPlayer mPlayerRef;
		private Color[] mOriginalColors;
		private Image[] mImages;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mImages = mImageHolder.GetComponentsInChildren<Image>();
			mOriginalColors = mImages.Select(x => x.color).ToArray();

			EventManager.Local.OnEnterAimDownSightsMode += OnEnterAimDownSightsMode;
			EventManager.Local.OnExitAimDownSightsMode += OnExitAimDownSightsMode;
			EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
			EventManager.Local.OnLocalPlayerCausedDamage += OnLocalPlayerCausedDamage;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnEnterAimDownSightsMode -= OnEnterAimDownSightsMode;
			EventManager.Local.OnExitAimDownSightsMode -= OnExitAimDownSightsMode;
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
			EventManager.Local.OnLocalPlayerCausedDamage -= OnLocalPlayerCausedDamage;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerSpawned
		/// Saves a reference to the player.
		/// </summary>
		private void OnLocalPlayerSpawned(CltPlayer obj)
		{
			mPlayerRef = obj;
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnEnterAimDownSightsMode
		/// </summary>
		private void OnEnterAimDownSightsMode()
		{
			mImageHolder.gameObject.SetActive(false);
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnExitAimDownSightsMode
		/// </summary>
		private void OnExitAimDownSightsMode()
		{
			mImageHolder.gameObject.SetActive(true);
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerCausedDamage
		/// </summary>
		private void OnLocalPlayerCausedDamage(float obj)
		{
			StopAllCoroutines();
			StartCoroutine(FadeBackColors(mFadeTime));
		}

		/// <summary>
		/// Unity's Update function
		/// </summary>
		private void Update()
		{
			// Ensure we have a player and a weapon.
			if (mPlayerRef == null)
				return;

			IWeapon weapon = mPlayerRef.weapon;
			if (weapon == null)
				return;

			// TODO: There has to be a better way to do this.
			// One per frame isn't the end of the world, but still...
			float scaleVal3 = Mathf.Tan(Mathf.Asin(weapon.GetCurrentDispersionFactor(true)));

			Vector3 cameraPos = mPlayerRef.eye.position;
			Vector3 rightPos = cameraPos + mPlayerRef.eye.forward + mPlayerRef.eye.right * scaleVal3;
			Vector3 upPos = cameraPos + mPlayerRef.eye.forward + mPlayerRef.eye.up * scaleVal3;

			Vector3 screenRight = Camera.main.WorldToScreenPoint(rightPos);
			Vector3 viewportRight = Camera.main.ScreenToViewportPoint(screenRight);
			Vector3 screenUp = Camera.main.WorldToScreenPoint(upPos);
			Vector3 viewportUp = Camera.main.ScreenToViewportPoint(screenUp);

			float width = Mathf.Abs(0.5f - viewportRight.x);
			float height = Mathf.Abs(0.5f - viewportUp.y);

			mImageHolder.anchorMax = new Vector2(0.5f + width, 0.5f + height);
			mImageHolder.anchorMin = new Vector2(0.5f - width, 0.5f - height);
		}

		/// <summary>
		/// Fade from red to the original color of the crosshair.
		/// </summary>
		private IEnumerator FadeBackColors(float time)
		{
			float currentTime = 0.0f;
			while (currentTime < time)
			{
				for (int i = 0; i < mImages.Length; i++)
					mImages[i].color = Color.Lerp(Color.red, mOriginalColors[i], currentTime / time);

				currentTime += Time.deltaTime;
				yield return null;
			}

			for (int i = 0; i < mImages.Length; i++)
				mImages[i].color = mOriginalColors[i];
		}
	}
}