using System;
using UnityEngine;

namespace FiringSquad.Data
{
	[Serializable]
	public class WeaponMovementData
	{
		[SerializeField] [Tooltip("The rate at which the weapon should follow the camera's positional movement.")]
		private float mCameraMovementFollowFactor = 10.0f;

		[SerializeField] [Tooltip("How many samples into the past the weapon should check for linear velocity.")]
		private int mPlayerPositionSamples = 5;

		[SerializeField] [Tooltip("How quickly the weapon bob \"cycles\" when active.")]
		private float mPlayerWeaponBobFrequency = 20f;

		[SerializeField] [Tooltip("How far the weapon moves left/right when bobbing.")]
		private float mPlayerWeaponBobXAmount = 0.2f;

		[SerializeField] [Tooltip("How far the weapon moves up/down when bobbing.")]
		private float mPlayerWeaponBobYAmount = 0.2f;

		[SerializeField] [Tooltip("How far the weapon moves forward/backward when bobbing.")]
		private float mPlayerWeaponBobZAmount = 0.2f;

		[SerializeField] [Tooltip("The rate at which the weapon should follow the camera's rotational movement.")]
		private float mCameraRotationFollowFactor = 15.0f;
		
		[SerializeField] [Tooltip("How many samples into the past the weapon should check for rotational velocity.")]
		private int mPlayerRotationSamples = 20;

		[SerializeField] [Tooltip("The curve that describes how to weight the averages of the past rotational velocity samples.")]
		private AnimationCurve mSampleWeighting;

		/// <summary>
		/// The rate at which the weapon should follow the camera's positional movement.
		/// </summary>
		public float cameraMovementFollowFactor { get { return mCameraMovementFollowFactor; } }

		/// <summary>
		/// How many samples into the past the weapon should check for linear velocity.
		/// </summary>
		public int playerPositionSamples { get { return mPlayerPositionSamples; } }

		/// <summary>
		/// How quickly the weapon bob "cycles" when active.
		/// </summary>
		public float playerWeaponBobFrequency { get { return mPlayerWeaponBobFrequency; } }

		/// <summary>
		/// How far the weapon moves left/right when bobbing.
		/// </summary>
		public float playerWeaponBobXAmount { get { return mPlayerWeaponBobXAmount; } }

		/// <summary>
		/// How far the weapon moves up/down when bobbing.
		/// </summary>
		public float playerWeaponBobYAmount { get { return mPlayerWeaponBobYAmount; } }

		/// <summary>
		/// How far the weapon moves forward/backward when bobbing.
		/// </summary>
		public float playerWeaponBobZAmount { get { return mPlayerWeaponBobZAmount; } }

		/// <summary>
		/// The rate at which the weapon should follow the camera's rotational movement.
		/// </summary>
		public float cameraRotationFollowFactor { get { return mCameraRotationFollowFactor; } }

		/// <summary>
		/// How many samples into the past the weapon should check for rotational velocity.
		/// </summary>
		public int playerRotationSamples { get { return mPlayerRotationSamples; } }

		/// <summary>
		/// The curve that describes how to weight the averages of the past rotational velocity samples.
		/// </summary>
		public AnimationCurve sampleWeighting { get { return mSampleWeighting; } }
	}
}
