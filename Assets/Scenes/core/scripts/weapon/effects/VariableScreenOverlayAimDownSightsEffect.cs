using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	[CreateAssetMenu(menuName = "reMod Data/ADS Effect - Variable Overlay")]
	public class VariableScreenOverlayAimDownSightsEffect : ScreenOverlayAimDownSightsEffect
	{
		[SerializeField] private float mMinZoom = 1.0f;
		[SerializeField] private float mMaxZoom = 3.0f;

		private float mZoomLevel = 15.0f;

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
		protected override void OnDestroy()
		{
			base.OnDestroy();
			EventManager.Local.OnZoomLevelChanged -= OnZoomLevelChanged;
		}

		/// <inheritdoc />
		protected override void ActivateStep2(Quickfade quickfade, WeaponPartScript part)
		{
			base.ActivateStep2(quickfade, part);
			EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(mZoomLevel, -1.0f));
		}

		/// <summary>
		/// Change zoom level based on scroll wheel
		/// </summary>
		/// <param name="val"></param>
		/// <param name="player">The player that provided this input.</param>
		private void OnZoomLevelChanged(float val, CltPlayer player)
		{
			// If the part is attached to a player's weapon
			if (mActive)
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
