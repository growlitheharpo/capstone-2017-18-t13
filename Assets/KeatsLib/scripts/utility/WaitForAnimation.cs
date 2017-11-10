using UnityEngine;

namespace KeatsLib.Unity
{
	/// <summary>
	/// Custom Yield Instruction that holds a Coroutine until the current animation on the provided animator finishes.
	/// </summary>
	public class WaitForAnimation : CustomYieldInstruction
	{
		private readonly Animator mAnim;
		private readonly int mNameHash;
		private readonly int mDepth;

		/// <summary>
		/// Custom Yield Instruction that holds a Coroutine until the current animation on the provided animator finishes.
		/// </summary>
		public WaitForAnimation(Animator anim, int depth = 0)
		{
			mAnim = anim;
			mDepth = depth;
			mNameHash = mAnim.GetCurrentAnimatorStateInfo(depth).fullPathHash;
		}

		public override bool keepWaiting { get { return mAnim.GetCurrentAnimatorStateInfo(mDepth).fullPathHash == mNameHash; } }
	}
}
