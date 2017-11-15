using System;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiringSquad.Data
{
	/// <summary>
	/// Small data class to represent the drop weights of a particular part type.
	/// </summary>
	[Serializable]
	public class WeaponDropWeights
	{
		/// Inspector variables
		[SerializeField] [Range(0.0f, 1.0f)] private float mMechanismWeight = 0.25f;
		[SerializeField] [Range(0.0f, 1.0f)] private float mBarrelWeight = 0.25f;
		[SerializeField] [Range(0.0f, 1.0f)] private float mScopeWeight = 0.25f;
		[SerializeField] [Range(0.0f, 1.0f)] private float mGripWeight = 0.25f;

		/// <summary>
		/// Weight to drop mechanisms (0-1)
		/// </summary>
		public float mechanismWeight { get { return mMechanismWeight; } }

		/// <summary>
		/// Weight to drop barrels (0-1)
		/// </summary>
		public float barrelWeight { get { return mBarrelWeight; } }

		/// <summary>
		/// Weight to drop scopes (0-1)
		/// </summary>
		public float scopeWeight { get { return mScopeWeight; } }

		/// <summary>
		/// Weight to drop grips (0-1)
		/// </summary>
		public float gripWeight { get { return mGripWeight; } }

		/// <summary>
		/// Choose a random attachment based on the weights in this instance.
		/// </summary>
		/// <returns></returns>
		public BaseWeaponScript.Attachment ChooseRandomWeightedAttachment()
		{
			float val = Random.value;
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
