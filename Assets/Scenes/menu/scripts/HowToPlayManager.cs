using UnityEngine;
using UIButton = UnityEngine.UI.Button;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// How to play menu slide manager
	/// </summary>
	public class HowToPlayManager : MonoBehaviour
	{
		[SerializeField] private UIButton mObjectiveButton;
		[SerializeField] private UIButton mControlsButton;
		[SerializeField] private UIButton mHudButton;
		[SerializeField] private UIButton mTipsButton;

		[SerializeField] private Animator mObjectiveAnimator;
		[SerializeField] private Animator mControlsAnimator;
		[SerializeField] private Animator mHudAnimator;
		[SerializeField] private Animator mTipsAnimator;

		private Animator mCurrentActiveAnimator;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mObjectiveButton.onClick.AddListener(() => SwitchToButton(mObjectiveAnimator));
			mControlsButton.onClick.AddListener(() => SwitchToButton(mControlsAnimator));
			mHudButton.onClick.AddListener(() => SwitchToButton(mHudAnimator));
			mTipsButton.onClick.AddListener(() => SwitchToButton(mTipsAnimator));

			ResetEverything();
		}

		/// <summary>
		/// Reset the how to play menu to the default state
		/// </summary>
		public void ResetEverything()
		{
			mObjectiveAnimator.SetBool("Enabled", false);
			mControlsAnimator.SetBool("Enabled", false);
			mHudAnimator.SetBool("Enabled", false);
			mTipsAnimator.SetBool("Enabled", false);
		}

		/// <summary>
		/// Switch to the provided new section and fade out our current one.
		/// </summary>
		/// <param name="newSection">The new section to fade to.</param>
		private void SwitchToButton(Animator newSection)
		{
			if (mCurrentActiveAnimator != null)
				mCurrentActiveAnimator.SetBool("Enabled", false);

			newSection.SetBool("Enabled", true);
			mCurrentActiveAnimator = newSection;
		}
	}
}

