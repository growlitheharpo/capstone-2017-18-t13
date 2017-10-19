using FiringSquad.Core.Audio;
using UnityEngine;

namespace KeatsLib.Unity
{
	public class WaitForAudio : CustomYieldInstruction
	{
		private readonly IAudioReference mRef;
		private readonly AudioSource mSource;

		public WaitForAudio(IAudioReference reference)
		{
			mRef = reference;
		}

		public WaitForAudio(AudioSource source)
		{
			mSource = source;
		}

		public override bool keepWaiting
		{
			get
			{
				if (mSource != null)
					return mSource.time + Time.deltaTime < mSource.clip.length;

				return mRef.isPlaying;
			}
		}
	}
}
