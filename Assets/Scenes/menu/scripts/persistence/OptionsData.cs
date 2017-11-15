// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using UnityEngine;

namespace FiringSquad.Data
{
	public class OptionsData
	{
		/// <summary>
		/// The background implementation for the IOptionsData package
		/// TODO: Re-add the persistence saving and loading that allows saving
		/// this data between play sessions.
		/// </summary>
		[Serializable]
		private class OptionsDataImpl : IOptionsData
		{
			[SerializeField] private float mFieldOfView;
			[SerializeField] private float mMasterVolume;
			[SerializeField] private float mMouseSensitivity;
			[SerializeField] private float mMusicVolume;
			[SerializeField] private float mSfxVolume;

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

		public static IOptionsData GetInstance()
		{
			return new OptionsDataImpl();
		}
	}
}
