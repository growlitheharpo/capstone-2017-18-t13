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
		protected bool mActive;
		private PostProcessVolume mTemporaryVolume;

		/// <summary>
		/// Unity's OnEnable function
		/// </summary>
		protected virtual void OnEnable()
		{
			mTemporaryVolume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("postprocessing"), 100);
		}

		/// <summary>
		/// Unity's OnDisable function
		/// </summary>
		protected override void OnDestroy()
		{
			RuntimeUtilities.DestroyVolume(mTemporaryVolume, true);
		}

		/// <inheritdoc />
		public override void ActivateEffect(IWeapon weapon, WeaponPartScript part)
		{
			if (mActive)
				return;

			mActive = true;
			base.ActivateEffect(weapon, part);

			Quickfade quickfade = CreateInstance<Quickfade>();
			quickfade.enabled.Override(true);
			quickfade.time.Override(mFadeTime);
			mTemporaryVolume.weight = 1.0f;
			mTemporaryVolume.profile.AddSettings(quickfade);

			EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(false));

			quickfade.Activate(part, false, () => ActivateStep2(quickfade, part));
		}

		/// <summary>
		/// Step 2 of activating our effect.
		/// Called after the fade TO black has completed.
		/// </summary>
		protected virtual void ActivateStep2(Quickfade quickfade, WeaponPartScript part)
		{
			if (!mActive)
				return;

			// called once we are faded to black
			Vignette vignette = CreateInstance<Vignette>();
			vignette.enabled.Override(true);
			vignette.intensity.Override(mVignetteIntensity);
			vignette.smoothness.Override(0.15f);
			vignette.rounded.Override(true);
			vignette.roundness.Override(1.0f);

			mTemporaryVolume.profile.AddSettings(vignette);
			EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(mTargetFieldOfView, -1.0f));

			quickfade.Deactivate(part, ActivateStep3);
		}

		/// <summary>
		/// Step 3 of activating our effect.
		/// Called after the fade FROM black has completed and our vignette is active.
		/// </summary>
		protected virtual void ActivateStep3()
		{
			if (!mActive)
				return;

			EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(true));

			if (mTemporaryVolume.profile.HasSettings<Quickfade>())
				mTemporaryVolume.profile.RemoveSettings<Quickfade>();
		}

		/// <inheritdoc />
		public override void DeactivateEffect(WeaponPartScript part, bool immediate)
		{
			if (!mActive)
				return;

			mActive = false;

			// Do this immediately so the new scope can do its effect if necessary
			EventManager.LocalGUI.RequestNewFieldOfView(-1.0f, -1.0f);
			foreach (PostProcessEffectSettings e in mTemporaryVolume.profile.settings)
			{
				if (e != null)
					Destroy(e);
			}

			mTemporaryVolume.profile.settings.Clear();
			mTemporaryVolume.profile.Reset();
		}
	}
}
