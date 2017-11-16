// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Options data holder.
	/// </summary>
	public class OptionsData
	{
		/// <summary>
		/// The background implementation for the IOptionsData package
		/// TODO: Re-add the persistence saving and loading that allows saving
		/// this data between play sessions.
		/// </summary>
		/// <inheritdoc />
		[Serializable]
		private class OptionsDataImpl : IOptionsData
		{
			/// Inspector/serialized variables
			[SerializeField] private float mFieldOfView;
			[SerializeField] private float mMasterVolume;
			[SerializeField] private float mMouseSensitivity;
			[SerializeField] private float mMusicVolume;
			[SerializeField] private float mSfxVolume;

			/// <inheritdoc />
			float IOptionsData.fieldOfView
			{
				get { return mFieldOfView; }
				set
				{
					if (mFieldOfView == value)
						return;

					mFieldOfView = value;
				}
			}

			/// <inheritdoc />
			float IOptionsData.masterVolume
			{
				get { return mMasterVolume; }
				set
				{
					if (mMasterVolume == value)
						return;

					mMasterVolume = value;
				}
			}

			/// <inheritdoc />
			public float sfxVolume
			{
				get { return mSfxVolume; }
				set
				{
					if (mSfxVolume == value)
						return;

					mSfxVolume = value;
				}
			}

			/// <inheritdoc />
			public float musicVolume
			{
				get { return mMusicVolume; }
				set
				{
					if (mMusicVolume == value)
						return;

					mMusicVolume = value;
				}
			}

			/// <inheritdoc />
			float IOptionsData.mouseSensitivity
			{
				get { return mMouseSensitivity; }
				set
				{
					if (mMouseSensitivity == value)
						return;

					mMouseSensitivity = value;
				}
			}
		}

		/// <summary>
		/// Create an IOptionsData instance.
		/// </summary>
		public static IOptionsData GetInstance()
		{
			return new OptionsDataImpl();
		}
	}
}
