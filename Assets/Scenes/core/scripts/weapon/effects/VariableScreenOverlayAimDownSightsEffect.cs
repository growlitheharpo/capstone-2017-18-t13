using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using KeatsLib.Unity;
using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Core.UI;
using FiringSquad.Data;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	[CreateAssetMenu(menuName = "reMod Data/ADS Effect - Variable Overlay")]
	public class VariableScreenOverlayAimDownSightsEffect : AimDownSightsEffect
	{
		/// Inspector variables
		[SerializeField] private float mTargetFieldOfView = 15.0f;
		[SerializeField] private float mFadeTime;
		[SerializeField] private float mVignetteIntensity = 1.0f;
		[SerializeField] private float mMinZoom = 1.0f;
		[SerializeField] private float mMaxZoom = 3.0f;
		[SerializeField] private float mZoomStep = 0.5f;

		private float mZoomLevel = 15.0f;

		/// Private variables
		private bool mActive;
		private Quickfade mQuickfade;
		private Vignette mVignette;
		private PostProcessVolume mTemporaryVolume;

		/// <summary>
		/// Unity's on awake function
		/// </summary>
		private void Awake()
		{
			EventManager.Local.OnZoomLevelChanged += OnZoomLevelChanged;
		}

		/// <summary>
		/// Unity's on destroy function
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnZoomLevelChanged -= OnZoomLevelChanged;
		}

		/// <inheritdoc />
		public override void ActivateEffect(IWeapon weapon, WeaponPartScript part)
		{
			if (mActive)
				return;

			mActive = true;
			base.ActivateEffect(weapon, part);

			mZoomLevel = 15.0f;

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
				EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(mZoomLevel, -1.0f));

				mQuickfade.Deactivate(part, () => EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(true)));
			});
		}

		/// <inheritdoc />
		public override void DeactivateEffect(WeaponPartScript part, bool immediate)
		{
			if (!mActive)
				return;

			mActive = false;

			mZoomLevel = 15.0f;


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

		/// <summary>
		/// Change zoom level based on scroll wheel
		/// </summary>
		/// <param name="val"></param>
		private void OnZoomLevelChanged(float val, CltPlayer player)
		{
			// If the part is attached to a player's weapon
			if(mActive)
			{
				if (val < 0)
				{
					mZoomLevel += 1.0f;

					mZoomLevel = Mathf.Clamp(mZoomLevel, mMinZoom, mMaxZoom);

					EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(mZoomLevel, -1.0f));
				}
				else if (val > 0)
				{
					mZoomLevel -= 1.0f;

					mZoomLevel = Mathf.Clamp(mZoomLevel, mMinZoom, mMaxZoom);

					EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(mZoomLevel, -1.0f));
				}
			} 
		}
	}
}
