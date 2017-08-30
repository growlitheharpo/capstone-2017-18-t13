// ReSharper disable CompareOfFloatsByEqualityOperator

using System;
using UnityEngine;

namespace KeatsLib.Persistence
{
	public partial class Persistence
	{
		[Serializable]
		private class OptionsDataImpl : BasePersisting, IOptionsData
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
					SetDirty();
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
					SetDirty();
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
					SetDirty();
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
					SetDirty();
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
					SetDirty();
				}
			}
		}
	}
}
