﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype2
{
	public class PlayerWeaponScript : MonoBehaviour
	{
		[Serializable]
		public struct WeaponData
		{
			[SerializeField] public float mDefaultSpread;
			[SerializeField] public float mDefaultDamage;
			[SerializeField] public float mFireRate;
			[SerializeField] [Range(0.0f, 1.0f)] public float mDefaultRecoil;

			public WeaponData(WeaponData other)
			{
				mDefaultSpread = other.mDefaultSpread;
				mDefaultDamage = other.mDefaultDamage;
				mFireRate = other.mFireRate;
				mDefaultRecoil = other.mDefaultRecoil;
			}

			public override string ToString()
			{
				return string.Format("Spread: {0}, Damage: {1}, FireRate: {2}, Recoil: {3}", mDefaultSpread, mDefaultDamage, mFireRate,
					mDefaultRecoil);
			}

			// Clip size, reload speed
		}

		public enum Attachment
		{
			Scope,
			Barrel,
		}

		[SerializeField] private WeaponData mBaseData;
		[SerializeField] private Transform mBarrelAttach;
		[SerializeField] private Transform mScopeAttach;

		private Dictionary<Attachment, Transform> mAttachPoints;
		private Dictionary<Attachment, WeaponPartScript> mCurrentAttachments;
		private WeaponData mCurrentData;

		private void Awake()
		{
			mAttachPoints = new Dictionary<Attachment, Transform>
			{
				{ Attachment.Scope, mScopeAttach },
				{ Attachment.Barrel, mBarrelAttach },
			};

			mCurrentAttachments = new Dictionary<Attachment, WeaponPartScript>(2);
			mCurrentData = new WeaponData(mBaseData);
		}

		public void AttachNewPart(Attachment place, WeaponPartScript part)
		{
			if (mCurrentAttachments.ContainsKey(place))
				Destroy(mCurrentAttachments[place].gameObject);

			part.transform.SetParent(mAttachPoints[place]);
			part.transform.localPosition = Vector3.zero;
			part.transform.localRotation = Quaternion.identity;

			mCurrentAttachments[place] = part;
			ActivatePartEffects();
		}

		private void ActivatePartEffects()
		{
			WeaponData start = new WeaponData(mBaseData);
			foreach (WeaponPartScript part in mCurrentAttachments.Values)
				start = part.ApplyEffects(start);

			mCurrentData = start;
		}

		public void FireWeapon()
		{
			//we'll do this later
			Debug.Log("Shoot! " + mCurrentData);
		}
	}
}
