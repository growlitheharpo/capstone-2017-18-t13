using UnityEngine;

public class WaitForParticles : CustomYieldInstruction
{
	private readonly ParticleSystem mParticles;

	public WaitForParticles(ParticleSystem ps)
	{
		mParticles = ps;
	}

	public override bool keepWaiting
	{
		get { return mParticles.isPlaying; }
	}
}
