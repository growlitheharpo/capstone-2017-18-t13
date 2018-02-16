using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	[CreateAssetMenu(menuName = "reMod Data/ADS Effect - Overlay")]
	public class ScreenOverlayAimDownSightsEffect : AimDownSightsEffect
	{
		/// Inspector variables
		[SerializeField] private float mTargetFieldOfView = 15.0f;
		[SerializeField] private float mFadeTime;
		[SerializeField] private float mVignetteIntensity = 1.0f;

		/// Private variables
		private bool mActive;
		private Quickfade mQuickfade;
		private Vignette mVignette;
		private PostProcessVolume mTemporaryVolume;

		/// <inheritdoc />
		public override void ActivateEffect(IWeapon weapon, WeaponPartScript part)
		{
			if (mActive)
				return;

			mActive = true;
			base.ActivateEffect(weapon, part);

			mQuickfade = CreateInstance<Quickfade>();
			mQuickfade.enabled.Override(true);
			mQuickfade.time.Override(mFadeTime);
			mTemporaryVolume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("postprocessing"), 100, mQuickfade);
			mTemporaryVolume.weight = 1.0f;

			EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(false));

			mQuickfade.Activate(part, false, () =>
			{
				// called once we are faded to black
				mVignette = CreateInstance<Vignette>();
				mVignette.enabled.Override(true);
				mVignette.intensity.Override(mVignetteIntensity);
				mVignette.smoothness.Override(0.15f);
				mVignette.rounded.Override(true);
				mVignette.roundness.Override(1.0f);

				mTemporaryVolume.profile.AddSettings(mVignette);
				EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(mTargetFieldOfView, -1.0f));

				mQuickfade.Deactivate(part, () => EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(true)));
			});
		}

		/// <inheritdoc />
		public override void DeactivateEffect(WeaponPartScript part, bool immediate)
		{
			if (!mActive)
				return;

			mActive = false;

			if (immediate)
			{
				// Do this immediately so the new scope can do its effect if necessary
				EventManager.LocalGUI.RequestNewFieldOfView(-1.0f, -1.0f);
				RuntimeUtilities.DestroyVolume(mTemporaryVolume, false);
				Destroy(mQuickfade);
				Destroy(mVignette);
				return;
			}

			// not immediate
			EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(false));
			mQuickfade.Activate(part, false, () =>
			{
				//called once we are faded to black
				mVignette.enabled.Override(false);
				Destroy(mVignette);

				// Do this immediately so the new scope can do its effect if necessary
				EventManager.LocalGUI.RequestNewFieldOfView(-1.0f, -1.0f);

				mQuickfade.Deactivate(part, () =>
				{
					EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(true));
					RuntimeUtilities.DestroyVolume(mTemporaryVolume, false);
					Destroy(mQuickfade);
					Destroy(mVignette);
				});
			});
		}
	}
}
