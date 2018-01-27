using System.Collections;
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
		[SerializeField] private RectTransform mHitMarkerHolder;
		[SerializeField] private float mFadeTime;

		/// Private variables
		private CltPlayer mPlayerRef;
		private Color[] mOriginalColors;
		private Color[] mHitOriginalColors;
		private Image[] mImages;
		private Image[] mHitImages;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mImages = mImageHolder.GetComponentsInChildren<Image>();
			mOriginalColors = mImages.Select(x => x.color).ToArray();

			// Do the same as the above for the hitmarkers
			mHitImages = mHitMarkerHolder.GetComponentsInChildren<Image>();
			mHitOriginalColors = mHitImages.Select(x => x.color).ToArray();

			EventManager.LocalGUI.OnSetCrosshairVisible += OnSetCrosshairVisible;
			EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
			EventManager.Local.OnLocalPlayerCausedDamage += OnLocalPlayerCausedDamage;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.LocalGUI.OnSetCrosshairVisible -= OnSetCrosshairVisible;
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
			EventManager.Local.OnLocalPlayerCausedDamage -= OnLocalPlayerCausedDamage;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerSpawned
		/// Saves a reference to the player.
		/// </summary>
		private void OnLocalPlayerSpawned(CltPlayer player)
		{
			mPlayerRef = player;
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
		}

		/// <summary>
		/// EVENT HANDLER: LocalGUI.OnSetCrosshairVisible
		/// </summary>
		private void OnSetCrosshairVisible(bool visible)
		{
			mImageHolder.gameObject.SetActive(visible);
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

				// Adding in the hit marker container as well
				for (int i = 0; i < mHitImages.Length; i++)
					mHitImages[i].color = Color.Lerp(Color.red, mHitOriginalColors[i], currentTime / time);

				currentTime += Time.deltaTime;
				yield return null;
			}

			for (int i = 0; i < mImages.Length; i++)
				mImages[i].color = mOriginalColors[i];

			for (int i = 0; i < mHitImages.Length; i++)
				mHitImages[i].color = mHitOriginalColors[i];
		}
	}
}
