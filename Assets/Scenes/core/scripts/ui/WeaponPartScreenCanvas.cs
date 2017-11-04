using System;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class WeaponPartScreenCanvas : MonoBehaviour
	{
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

		private WeaponPartScript mCurrentPart;
		private IWeapon mPlayerWeaponRef;

		private void Awake()
		{
			EventManager.Local.OnLocalPlayerAttachedPart += OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerHoldingPart += OnLocalPlayerHoldingPart;
			EventManager.Local.OnLocalPlayerReleasedPart += OnLocalPlayerReleasedPart;

			mTotalArea.SetActive(false);
		}

		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerAttachedPart -= OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerHoldingPart -= OnLocalPlayerHoldingPart;
			EventManager.Local.OnLocalPlayerReleasedPart -= OnLocalPlayerReleasedPart;
		}

		private void OnLocalPlayerHoldingPart(WeaponPartScript part)
		{
			mTotalArea.SetActive(true);
			mCurrentPart = part;
			CalculateAndDisplayStats();
		}

		private void CalculateAndDisplayStats()
		{
			WeaponPartCollection fakeCollection = new WeaponPartCollection(mPlayerWeaponRef.currentParts);
			fakeCollection[mCurrentPart.attachPoint] = mCurrentPart;

			WeaponData currentData = mPlayerWeaponRef.currentData;
			WeaponData newData = BaseWeaponScript.ActivatePartEffects(fakeCollection, mPlayerWeaponRef.baseData);

			mPartName.text = mCurrentPart.prettyName;
			mDamageIcon.sprite = ChooseSprite(currentData.damage, newData.damage);
			mRangeIcon.sprite = ChooseSprite(currentData.damageFalloffDistance, newData.damageFalloffDistance);
			mFireRateIcon.sprite = ChooseSprite(currentData.fireRate, newData.fireRate);
			mClipSizeIcon.sprite = ChooseSprite(currentData.clipSize, newData.clipSize);
			// note: accuracy is flipped on purpose because a higher number means it's worse, unlike everything else
			mAccuracyIcon.sprite = ChooseSprite(newData.maximumDispersion, currentData.maximumDispersion);
		}

		private void OnLocalPlayerReleasedPart(WeaponPartScript obj)
		{
			mTotalArea.SetActive(false);
		}

		private void OnLocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript part)
		{
			mCurrentPart = null;
			mPlayerWeaponRef = weapon;
		}
		
		private Sprite ChooseSprite(float oldValue, float newValue)
		{
			if (Math.Abs(oldValue - newValue) < 0.05f)
				return mNeutralSprite;

			return oldValue > newValue ? mNegativeSprite : mPositiveSprite;
		}
		
		private void Update()
		{
			if (!mTotalArea.activeInHierarchy)
				return;

			if (mCurrentPart == null)
				mTotalArea.SetActive(false);
		}
	}
}
