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
		[SerializeField] private float mFadeTime;

		/// Private variables
		private CltPlayer mPlayerRef;
		private Color[] mOriginalColors;
		private Image[] mImages;
		private GameObject mMarker;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mImages = mImageHolder.GetComponentsInChildren<Image>();
			mOriginalColors = mImages.Select(x => x.color).ToArray();
			mMarker = Resources.Load<GameObject>("prefabs/ui/p_animated-hit-indicator");

			EventManager.LocalGUI.OnSetCrosshairVisible += OnSetCrosshairVisible;
			EventManager.Local.OnLocalPlayerCausedDamage += OnLocalPlayerCausedDamage;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.LocalGUI.OnSetCrosshairVisible -= OnSetCrosshairVisible;
			EventManager.Local.OnLocalPlayerCausedDamage -= OnLocalPlayerCausedDamage;
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

			GameObject tmpMarker;

			// Spawn a hitmarker
			tmpMarker = Instantiate(mMarker, this.transform);
			tmpMarker.transform.localPosition = new Vector3(0, 0, 0);
			tmpMarker.transform.localScale = new Vector3(.5f, .5f, .5f);
		}

		/// <summary>
		/// Unity's Update function
		/// </summary>
		private void Update()
		{
			SearchForPlayer();

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
		/// Attempts to grab a reference to the local player. Runs every frame until success.
		/// Acceptable because there is only one crosshair in the scene.
		/// </summary>
		private void SearchForPlayer()
		{
			if (mPlayerRef != null)
				return;

			mPlayerRef = FindObjectsOfType<CltPlayer>().FirstOrDefault(x => x.isCurrentPlayer);
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
