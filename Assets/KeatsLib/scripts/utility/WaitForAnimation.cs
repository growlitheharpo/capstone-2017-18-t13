using UnityEngine;

namespace KeatsLib.Unity
{
	public class WaitForAnimation : CustomYieldInstruction
	{
		private readonly Animator mAnim;
		private readonly int mNameHash;
		private readonly int mDepth;

		public WaitForAnimation(Animator anim, int depth = 0)
		{
			mAnim = anim;
			mDepth = depth;
			mNameHash = mAnim.GetCurrentAnimatorStateInfo(depth).fullPathHash;
		}

		public override bool keepWaiting { get { return mAnim.GetCurrentAnimatorStateInfo(mDepth).fullPathHash == mNameHash; } }
	}
}
