using System;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Data
{
	[Serializable]
	public class WeaponDropWeights
	{
		[SerializeField] [Range(0.0f, 1.0f)] private float mMechanismWeight = 0.25f;
		[SerializeField] [Range(0.0f, 1.0f)] private float mBarrelWeight = 0.25f;
		[SerializeField] [Range(0.0f, 1.0f)] private float mScopeWeight = 0.25f;
		[SerializeField] [Range(0.0f, 1.0f)] private float mGripWeight = 0.25f;

		public float mechanismWeight { get { return mMechanismWeight; } }
		public float barrelWeight { get { return mBarrelWeight; } }
		public float scopeWeight { get { return mScopeWeight; } }
		public float gripWeight { get { return mGripWeight; } }

		public BaseWeaponScript.Attachment ChooseRandomWeightedAttachment()
		{
			float val = UnityEngine.Random.value;
			if (val < mMechanismWeight)
				return BaseWeaponScript.Attachment.Mechanism;
			if (val < mMechanismWeight + mBarrelWeight)
				return BaseWeaponScript.Attachment.Barrel;
			if (val < mMechanismWeight + mBarrelWeight + mScopeWeight)
				return BaseWeaponScript.Attachment.Scope;

			return BaseWeaponScript.Attachment.Grip;
		}
	}
}
