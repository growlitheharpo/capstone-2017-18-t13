using System;
using System.Collections.Generic;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using KeatsLib.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

public class WeaponPartScreenCanvas : MonoBehaviour
{
	[Serializable]
	private class UIPiece
	{
		[SerializeField] private UIImage mDifferenceBar;
		[SerializeField] private UIImage mFillBar;
		[SerializeField] private UIImage mArrow;

		public UIImage differenceBar { get { return mDifferenceBar; } }
		public UIImage fillBar { get { return mFillBar; } }
		public UIImage arrow { get { return mArrow; } }

		public UIPiece(UIImage diff, UIImage fill, UIImage arrow)
		{
			mDifferenceBar = diff;
			mFillBar = fill;
			mArrow = arrow;
		}
		
		public void SetAmount(float p, bool isBase)
		{
			if (isBase)
			{
				differenceBar.fillAmount = p;
				fillBar.fillAmount = p;
				arrow.rectTransform.anchorMin = new Vector2(p, 0.0f);
				arrow.rectTransform.anchorMax = new Vector2(p, 1.0f);
			}
			else if (fillBar.fillAmount < p) // part increases stat
			{
				differenceBar.fillAmount = p;
				arrow.rectTransform.anchorMin = new Vector2(p, 0.0f);
				arrow.rectTransform.anchorMax = new Vector2(p, 1.0f);
			}
			else
			{
				fillBar.fillAmount = p;
				arrow.rectTransform.anchorMin = new Vector2(p, 0.0f);
				arrow.rectTransform.anchorMax = new Vector2(p, 1.0f);
				arrow.rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
			}
		}
	}

	[SerializeField] private GameObject mDamageBar;
	[SerializeField] private GameObject mAccuracyBar;
	[SerializeField] private GameObject mRangeBar;
	[SerializeField] private GameObject mFireRateBar;
	[SerializeField] private GameObject mReloadRateBar;
	[SerializeField] private GameObject mRecoilBar;
	[SerializeField] private GameObject mClipSizeBar;

	private Dictionary<GameObject, UIPiece> mUIDictionary;
	private WeaponData mCurrentData;

	private void Awake()
	{
		CreateUIDictionary();
		EventManager.Local.OnLocalPlayerAttachedPart += OnLocalPlayerAttachedPart;
	}

	private void CreateUIDictionary()
	{
		mUIDictionary = new Dictionary<GameObject, UIPiece>();
		var stats = new[] { mDamageBar, mAccuracyBar, mRangeBar, mFireRateBar, mReloadRateBar, mRecoilBar, mClipSizeBar };
		foreach (GameObject stat in stats)
		{
			UIImage diff = stat.transform.Find("UIFillBar").Find("_difference").GetComponent<UIImage>();
			UIImage fill = stat.transform.Find("UIFillBar").Find("_top").GetComponent<UIImage>();
			UIImage arrow = stat.transform.Find("UIFillBar").Find("_arrow").GetComponent<UIImage>();

			mUIDictionary.Add(stat, new UIPiece(diff, fill, arrow));
		}
	}

	private void OnLocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript part)
	{
		mCurrentData = weapon.currentData;
		SetStats(mCurrentData, true);
	}

	public void PickUpPart(WeaponPartScript part)
	{
		WeaponData fakeData = new WeaponData(mCurrentData, part.data[0]);
		SetStats(fakeData, false);
	}

	private void SetStats(WeaponData data, bool setBase)
	{
		float damage = data.damage.Rescale(0.0f, 150.0f);
		float accuracy = 1.0f - data.maximumDispersion.Rescale(0.0f, 0.3f);
		float range = data.damageFalloffDistance.Rescale(0.0f, 200.0f);
		float fireRate = data.fireRate.Rescale(0.0f, 13.0f);
		float reloadTime = data.reloadTime.Rescale(0.0f, 4.0f);
		float recoil = data.recoilAmount.Rescale(0.0f, 30.0f);
		float clipSize = ((float)data.clipSize).Rescale(0.0f, 60.0f);

		mUIDictionary[mDamageBar].SetAmount(damage, setBase);
		mUIDictionary[mAccuracyBar].SetAmount(accuracy, setBase);
		mUIDictionary[mRangeBar].SetAmount(range, setBase);
		mUIDictionary[mFireRateBar].SetAmount(fireRate, setBase);
		mUIDictionary[mReloadRateBar].SetAmount(reloadTime, setBase);
		mUIDictionary[mRecoilBar].SetAmount(recoil, setBase);
		mUIDictionary[mClipSizeBar].SetAmount(clipSize, setBase);
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
