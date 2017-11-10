using FiringSquad.Core.Audio;
using UnityEngine;

namespace KeatsLib.Unity
{
	/// <summary>
	/// Custom Yield Instruction that holds a Coroutine until the provided audio finishes.
	/// </summary>
	public class WaitForAudio : CustomYieldInstruction
	{
		private readonly IAudioReference mRef;

		/// <summary>
		/// Custom Yield Instruction that holds a Coroutine until the provided audio finishes.
		/// </summary>
		public WaitForAudio(IAudioReference reference)
		{
			mRef = reference;
		}
		
		public override bool keepWaiting
		{
			get
			{
				return mRef.isPlaying;
			}
		}
	}
}
