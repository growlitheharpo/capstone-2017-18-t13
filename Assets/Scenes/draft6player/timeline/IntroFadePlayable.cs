using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace FiringSquad.Gameplay.Timeline
{
	/// <summary>
	/// Utility Playable that fades the screen to black and back when enabled
	/// </summary>
	public class IntroFadePlayable : MonoBehaviour
	{
		// Inspector variables
		[SerializeField] private float mFadeOutTime;
		[SerializeField] private float mFadeInTime;

		/// Private variables
		private PostProcessVolume mTemporaryVolume;
		private Quickfade mQuickfadeInstance;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mTemporaryVolume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("postprocessing"), 110);
			mTemporaryVolume.weight = 1.0f;

			mQuickfadeInstance = ScriptableObject.CreateInstance<Quickfade>();
			mTemporaryVolume.profile.AddSettings(mQuickfadeInstance);
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			RuntimeUtilities.DestroyVolume(mTemporaryVolume, true);

			if (mQuickfadeInstance != null)
				Destroy(mQuickfadeInstance);
		}

		/// <summary>
		/// Unity's OnEnable function.
		/// </summary>
		private void OnEnable()
		{
			StopAllCoroutines();
			mQuickfadeInstance.enabled.Override(true);
			mQuickfadeInstance.time.Override(mFadeInTime);
			mQuickfadeInstance.Activate(this, false, OnQuickfadeInComplete);
		}

		/// <summary>
		/// Called after the intro has faded in.
		/// </summary>
		private void OnQuickfadeInComplete()
		{
			mQuickfadeInstance.time.Override(mFadeOutTime);
			mQuickfadeInstance.Deactivate(this);
		}
	}
}
