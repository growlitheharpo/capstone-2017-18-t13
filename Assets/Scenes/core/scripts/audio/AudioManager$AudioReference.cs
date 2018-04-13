using System;
using FMOD;
using FMOD.Studio;
using UnityEngine;

namespace FiringSquad.Core.Audio
{
	public partial class AudioManager
	{
		/// <summary>
		/// Private implementation of the IAudioReference interface.
		/// </summary>
		/// <inheritdoc />
		private class AudioReference : IAudioReference
		{
			private EventInstance mEvent;

			/// <inheritdoc />
			public bool isPlaying
			{
				get
				{
					PLAYBACK_STATE state;
					mEvent.getPlaybackState(out state);
					return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING;
				}
			}

			/// <inheritdoc />
			public AudioReference(EventInstance e)
			{
				mEvent = e;
			}

			/// <inheritdoc />
			public IAudioReference Start()
			{
				mEvent.start();
				return this;
			}

			/// <inheritdoc />
			public IAudioReference Kill(bool allowFade = true)
			{
				mEvent.stop(allowFade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
				mEvent.release();
				return this;
			}

			/// <inheritdoc />
			public IAudioReference SetVolume(float vol)
			{
				mEvent.setVolume(vol);
				return this;
			}

			/// <inheritdoc />
			public IAudioReference AttachToRigidbody(Rigidbody rb)
			{
				FMODUnity.RuntimeManager.AttachInstanceToGameObject(mEvent, rb.transform, rb);
				return this;
			}

			/// <summary>
			/// True if this reference is valid and currently playing.
			/// </summary>
			public bool isAlive
			{
				get { return mEvent.isValid() && mEvent.hasHandle() && isPlaying; }
			}

			/// <inheritdoc />
			public float playerSpeed { get { return GetParameter("PlayerSpeed"); } set { SetParameter("PlayerSpeed", value); } }

			/// <inheritdoc />
			public float countDownTimeRemaining { get { return GetParameter("Timer"); } set { SetParameter("Timer", value); } }

			/// <inheritdoc />
			public float usingRocketBooster { get { return GetParameter("usingRocketBooster"); } set { SetParameter("usingRocketBooster", value); } }

			/// <inheritdoc />
			public float isSprinting { get { return GetParameter("isRunning"); } set { SetParameter("isRunning", value); } }

			/// <inheritdoc />
			public float weaponType { get { return GetParameter("WeaponType"); } set { SetParameter("WeaponType", value); } }

			/// <inheritdoc />
			public float barrelType { get { return GetParameter("BarrelType"); } set { SetParameter("BarrelType", value); } }

			/// <inheritdoc />
			public float isCurrentPlayer { get { return GetParameter("IsCurrentPlayer"); } set { SetParameter("IsCurrentPlayer", value); } }

			/// <inheritdoc />
			public float healthGained { get { return GetParameter("HealthGained"); } set { SetParameter("HealthGained", value); } }

			/// <inheritdoc />
			public float crowdHypeLevel { get { return GetParameter("CrowdHypeLevel"); } set { SetParameter("CrowdHypeLevel", value); } }

			/// <inheritdoc />
			public float isPlayButton { get { return GetParameter("IsPlayButton"); } set { SetParameter("IsPlayButton", value); } }

			/// <inheritdoc />
			public IAudioReference SetParameter(string name, float value)
			{
				RESULT result = mEvent.setParameterValue(name, value);
				if (result != RESULT.OK)
				{
					throw new ArgumentException(
						string.Format("Could not set parameter: {0} value {1:##.000}. Result was: {2}", name, value, result.ToString()));
				}

				return this;
			}

			/// <inheritdoc />
			public float GetParameter(string name)
			{
				ParameterInstance instance;
				RESULT result = mEvent.getParameter(name, out instance);
				if (result != RESULT.OK)
					throw new ArgumentException(string.Format("Could not get parameter: {0}. Result was: {1}", name, result.ToString()));

				float val;
				result = instance.getValue(out val);
				if (result != RESULT.OK)
					throw new ArgumentException(string.Format("Could not get parameter: {0}. Result was: {1}", name, result.ToString()));

				return val;
			}
		}
	}
}
