using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that manages the display of the player's durability on the UI.
	/// </summary>
	public class DurabilityIndicator : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private UIImage mMech;
		[SerializeField] private UIImage mBarrel;
		[SerializeField] private UIImage mGrip;
		[SerializeField] private UIImage mScope;

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
			switch (part.attachPoint)
			{
				case Attachment.Scope:
					mScope.sprite = part.durabilitySprite;
					mScope.color = Color.white;
					break;
				case Attachment.Barrel:
					mBarrel.sprite = part.durabilitySprite;
					mBarrel.color = Color.white;
					break;
				case Attachment.Mechanism:
					mMech.sprite = part.durabilitySprite;
					mMech.color = Color.white;
					break;
				case Attachment.Grip:
					mGrip.sprite = part.durabilitySprite;
					mMech.color = Color.white;
					break;
			}
		}
	}
}
