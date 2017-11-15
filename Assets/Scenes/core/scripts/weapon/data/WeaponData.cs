using System;
using FiringSquad.Networking;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace FiringSquad.Data
{
	/// <summary>
	/// Data holder for all of the "stats" data related to a weapon instance.
	/// </summary>
	[Serializable]
	public struct WeaponData : INetworkable<WeaponData>
	{
		/// Inspector variables
		[FormerlySerializedAs("mSpread")] [SerializeField] private float mMinimumDispersion;
		[SerializeField] private float mMaximumDispersion;
		[SerializeField] private float mDispersionRamp;

		[FormerlySerializedAs("mRecoil")] [SerializeField] private float mRecoilAmount;
		[SerializeField] private float mRecoilTime;
		[SerializeField] private AnimationCurve mRecoilCurve;

		[SerializeField] private float mDamage;
		[SerializeField] private float mDamageFalloffDistance;
		[SerializeField] private float mFireRate;
		[SerializeField] private float mReloadTime;
		[SerializeField] private int mClipSize;

		public float minimumDispersion { get { return mMinimumDispersion; } }
		public float maximumDispersion { get { return mMaximumDispersion; } }
		public float dispersionRamp { get { return mDispersionRamp; } }

		public float recoilAmount { get { return mRecoilAmount; } }
		public float recoilTime { get { return mRecoilTime; } }
		public AnimationCurve recoilCurve { get { return mRecoilCurve; } }

		public float damage { get { return mDamage; } }
		public float damageFalloffDistance { get { return mDamageFalloffDistance; } }
		public float fireRate { get { return mFireRate; } }
		public float reloadTime { get { return mReloadTime; } }
		public int clipSize { get { return mClipSize; } }

		/// <summary>
		/// Data holder for all of the "stats" data related to a weapon instance.
		/// </summary>
		/// <param name="other">The WeaponData to fully copy.</param>
		public WeaponData(WeaponData other)
		{
			mMinimumDispersion = other.mMinimumDispersion;
			mMaximumDispersion = other.mMaximumDispersion;
			mDispersionRamp = other.mDispersionRamp;
			mRecoilAmount = other.mRecoilAmount;
			mRecoilTime = other.mRecoilTime;
			mRecoilCurve = new AnimationCurve(other.mRecoilCurve.keys);
			mDamage = other.mDamage;
			mDamageFalloffDistance = other.mDamageFalloffDistance;
			mFireRate = other.mFireRate;
			mClipSize = other.mClipSize;
			mReloadTime = other.mReloadTime;
		}

		/// <summary>
		/// Data holder for all of the "stats" data related to a weapon instance.
		/// </summary>
		/// <param name="other">The WeaponData to copy.</param>
		/// <param name="data">The weapon part whose modifiers will be applied to the copy.</param>
		public WeaponData(WeaponData other, WeaponPartData data)
		{
			mDamage = data.damageModifier.Apply(other.mDamage);
			mDamageFalloffDistance = data.damageFalloffDistanceModifier.Apply(other.mDamageFalloffDistance);
			mMinimumDispersion = data.minDispersionModifier.Apply(other.mMinimumDispersion);
			mMaximumDispersion = data.maxDispersionModifier.Apply(other.mMaximumDispersion);
			mDispersionRamp = data.dispersionRampModifier.Apply(other.mDispersionRamp);
			mRecoilAmount = data.recoilAmountModifier.Apply(other.mRecoilAmount);
			mRecoilTime = data.recoilTimeModifier.Apply(other.mRecoilTime);
			mRecoilCurve = new AnimationCurve(other.mRecoilCurve.keys);
			mClipSize = data.clipModifier.Apply(other.mClipSize);
			mFireRate = data.fireRateModifier.Apply(other.mFireRate);
			mReloadTime = data.reloadModifier.Apply(other.mReloadTime);
		}

		/// <inheritdoc />
		public void Serialize(NetworkWriter stream)
		{
			stream.Write(mMinimumDispersion);
			stream.Write(mMaximumDispersion);
			stream.Write(mDispersionRamp);

			stream.Write(mRecoilAmount);
			stream.Write(mRecoilTime);

			stream.Write(mDamage);
			stream.Write(mDamageFalloffDistance);

			stream.Write(mFireRate);
			stream.Write(mReloadTime);
			stream.Write(mClipSize);
		}

		/// <inheritdoc />
		public void Deserialize(NetworkReader stream, out object target)
		{
			target = new WeaponData
			{
				mMinimumDispersion = stream.ReadSingle(),
				mMaximumDispersion = stream.ReadSingle(),
				mDispersionRamp = stream.ReadSingle(),

				mRecoilAmount = stream.ReadSingle(),
				mRecoilTime = stream.ReadSingle(),

				mDamage = stream.ReadSingle(),
				mDamageFalloffDistance = stream.ReadSingle(),

				mFireRate = stream.ReadSingle(),
				mReloadTime = stream.ReadSingle(),
				mClipSize = stream.ReadInt32()
			};
		}

		/// <inheritdoc />
		public WeaponData Deserialize(NetworkReader reader)
		{
			object result;
			Deserialize(reader, out result);
			this = (WeaponData)result;
			return this;
		}
		
		/// <summary>
		/// Immediately modify the minimumDispersion property.
		/// </summary>
		public void ForceModifyMinDispersion(Modifier.Float modification)
		{
			mMinimumDispersion = modification.Apply(mMinimumDispersion);
		}

		/// <summary>
		/// Immediately modify the maximumDispersion property.
		/// </summary>
		public void ForceModifyMaxDispersion(Modifier.Float modification)
		{
			mMaximumDispersion = modification.Apply(mMaximumDispersion);
		}

		/// <summary>
		/// Immediately modify the dispersionRamp property.
		/// </summary>
		public void ForceModifyDispersionRamp(Modifier.Float modification)
		{
			mDispersionRamp = modification.Apply(mDispersionRamp);
		}

		/// <summary>
		/// Immediately modify the recoilAmount property.
		/// </summary>
		public void ForceModifyRecoilAmount(Modifier.Float modification)
		{
			mRecoilAmount = modification.Apply(mRecoilAmount);
		}

		/// <summary>
		/// Immediately modify the recoilTime property.
		/// </summary>
		public void ForceModifyRecoilTime(Modifier.Float modification)
		{
			mRecoilTime = modification.Apply(mRecoilTime);
		}

		/// <summary>
		/// Immediately modify the damage property.
		/// </summary>
		public void ForceModifyDamage(Modifier.Float modification)
		{
			mDamage = modification.Apply(mDamage);
		}

		/// <summary>
		/// Immediately modify the damageFalloffDistance property.
		/// </summary>
		public void ForceModifyDamageFalloff(Modifier.Float modification)
		{
			mDamageFalloffDistance = modification.Apply(mDamageFalloffDistance);
		}

		/// <summary>
		/// Immediately modify the fireRate property.
		/// </summary>
		public void ForceModifyFireRate(Modifier.Float modification)
		{
			mFireRate = modification.Apply(mFireRate);
		}

		/// <summary>
		/// Immediately modify the reloadTime property.
		/// </summary>
		public void ForceModifyReloadTime(Modifier.Float modification)
		{
			mReloadTime = modification.Apply(mReloadTime);
		}

		/// <summary>
		/// Immediately modify the clipSize property.
		/// </summary>
		public void ForceModifyClipSize(Modifier.Int modification)
		{
			mClipSize = modification.Apply(mClipSize);
		}
	}
}
