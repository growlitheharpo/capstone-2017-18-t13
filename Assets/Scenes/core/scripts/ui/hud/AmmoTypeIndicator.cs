using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that manages the display of the player's ammo type on the HUD.
	/// </summary>
	public class AmmoTypeIndicator : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private UIImage mAmmoImage;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			EventManager.Local.OnLocalPlayerAttachedPart += OnLocalPlayerAttachedPart;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerAttachedPart -= OnLocalPlayerAttachedPart;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerAttachedPart
		/// </summary>
		private void OnLocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript part)
		{
			if (part.attachPoint != Attachment.Mechanism)
				return;

			WeaponPartScriptMechanism realPart = part as WeaponPartScriptMechanism;
			if (realPart != null)
				mAmmoImage.sprite = realPart.ammoTypeSprite;
		}
	}
}
