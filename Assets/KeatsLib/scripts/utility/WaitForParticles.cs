using UnityEngine;

namespace KeatsLib.Unity
{
	/// <summary>
	/// Custom Yield Instruction that holds a Coroutine until the provided particle system finishes.
	/// </summary>
	public class WaitForParticles : CustomYieldInstruction
	{
		private readonly ParticleSystem mParticles;

		/// <summary>
		/// Custom Yield Instruction that holds a Coroutine until the provided particle system finishes.
		/// </summary>
		public WaitForParticles(ParticleSystem ps)
		{
			mParticles = ps;
		}

		public override bool keepWaiting { get { return mParticles.isPlaying; } }
	}
}
