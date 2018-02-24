using System;
using System.Collections;
using System.Linq;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that handles displaying a weapon part's stats on screen.
	/// </summary>
	public class WeaponPartScreenCanvas : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mTotalArea;
		[SerializeField] private UIText mPartName;
		[SerializeField] private UIImage mDamageIcon;
		[SerializeField] private UIImage mAccuracyIcon;
		[SerializeField] private UIImage mRangeIcon;
		[SerializeField] private UIImage mFireRateIcon;
		[SerializeField] private UIImage mClipSizeIcon;

		[SerializeField] private Sprite mNeutralSprite;
		[SerializeField] private Sprite mPositiveSprite;
		[SerializeField] private Sprite mNegativeSprite;

		/// Private variables
		private WeaponPartScript mCurrentPart;
		private IWeapon mPlayerWeaponRef;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			EventManager.Local.OnLocalPlayerAttachedPart += OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerHoldingPart += OnLocalPlayerHoldingPart;
			EventManager.Local.OnLocalPlayerReleasedPart += OnLocalPlayerReleasedPart;

			StartCoroutine(GrabPlayerReference());

			mTotalArea.SetActive(false);
		}

		/// <summary>
		/// Cleanup all listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerAttachedPart -= OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerHoldingPart -= OnLocalPlayerHoldingPart;
			EventManager.Local.OnLocalPlayerReleasedPart -= OnLocalPlayerReleasedPart;
		}

		/// <summary>
		/// Grab a reference to the local player's weapon.
		/// </summary>
		private IEnumerator GrabPlayerReference()
		{
			while (mPlayerWeaponRef == null)
			{
				yield return null;
				CltPlayer script = CltPlayer.localPlayerReference;

				if (script == null)
					continue;

				mPlayerWeaponRef = (BaseWeaponScript)script.weapon;
			}
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerHoldingPart
		/// </summary>
		private void OnLocalPlayerHoldingPart(WeaponPartScript part)
		{
			mTotalArea.SetActive(true);
			mCurrentPart = part;
			CalculateAndDisplayStats();
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerReleasedPart
		/// </summary>
		private void OnLocalPlayerReleasedPart(WeaponPartScript obj)
		{
			mTotalArea.SetActive(false);
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerAttachedPart
		/// </summary>
		private void OnLocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript part)
		{
			mCurrentPart = null;
		}

		/// <summary>
		/// Calculate the change in stats for each of the weapon categories then display them on the panel.
		/// </summary>
		private void CalculateAndDisplayStats()
		{
			WeaponPartCollection fakeCollection = new WeaponPartCollection(mPlayerWeaponRef.currentParts);
			fakeCollection[mCurrentPart.attachPoint] = mCurrentPart;

			WeaponData currentData = mPlayerWeaponRef.currentData;
			WeaponData newData = WeaponData.ActivatePartEffects(mPlayerWeaponRef.baseData, fakeCollection);

			mPartName.text = mCurrentPart.prettyName;
			mDamageIcon.sprite = ChooseSprite(currentData.damage, newData.damage);
			mRangeIcon.sprite = ChooseSprite(currentData.damageFalloffDistance, newData.damageFalloffDistance);
			mFireRateIcon.sprite = ChooseSprite(currentData.fireRate, newData.fireRate);
			mClipSizeIcon.sprite = ChooseSprite(currentData.clipSize, newData.clipSize);
			// note: accuracy is flipped on purpose because a higher number means it's worse, unlike everything else
			mAccuracyIcon.sprite = ChooseSprite(newData.maximumDispersion, currentData.maximumDispersion);
		}

		/// <summary>
		/// Chooses which sprite to display based on the relation of newValue to oldValue.
		/// </summary>
		/// <param name="oldValue">The current value of the stat.</param>
		/// <param name="newValue">The hypothetical value of the stat if this part were applied.</param>
		/// <returns>The neutral sprite if they are roughly equal; the negativeSprite if 
		/// oldValue is higher, the positiveSprite if newValue is higher.</returns>
		private Sprite ChooseSprite(float oldValue, float newValue)
		{
			if (Math.Abs(oldValue - newValue) < 0.05f)
				return mNeutralSprite;

			return oldValue > newValue ? mNegativeSprite : mPositiveSprite;
		}
		
		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			if (!mTotalArea.activeInHierarchy)
				return;

			if (mCurrentPart == null)
				mTotalArea.SetActive(false);
		}
	}
}
