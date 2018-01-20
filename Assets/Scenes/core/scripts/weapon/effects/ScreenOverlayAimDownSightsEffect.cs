﻿using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	[CreateAssetMenu(menuName = "reMod Data/ADS Effect - Overlay")]
	public class ScreenOverlayAimDownSightsEffect : AimDownSightsEffect
	{
		/// Private variables
		private Quickfade mVignette;
		private PostProcessVolume mTemporaryVolume;

		/// <inheritdoc />
		public override void ActivateEffect(IWeapon weapon, WeaponPartScript part)
		{
			base.ActivateEffect(weapon, part);

			EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(15.0f, -1.0f));

			// Create a temporary effect with all of its necessary settings
			/*mVignette = CreateInstance<Vignette>();
			mVignette.enabled.Override(true);
			mVignette.intensity.Override(1.0f);
			mVignette.smoothness.Override(0.15f);
			mVignette.rounded.Override(true);
			mVignette.roundness.Override(1.0f);*/

			mVignette = CreateInstance<Quickfade>();
			mVignette.enabled.Override(true);
			mVignette.time.Override(1.0f);
			//mVignette.blend.Override(1.0f);
			mVignette.Activate(part);

			mTemporaryVolume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("postprocessing"), 100, mVignette);
			mTemporaryVolume.weight = 1.0f;
		}

		/// <inheritdoc />
		public override void DeactivateEffect(IWeapon weapon, WeaponPartScript part, bool immediate)
		{
			EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(-1.0f, -1.0f));

			RuntimeUtilities.DestroyVolume(mTemporaryVolume, false);
			Destroy(mVignette);
		}
	}
}
