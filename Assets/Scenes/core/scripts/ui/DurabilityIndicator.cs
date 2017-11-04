using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	public class DurabilityIndicator : MonoBehaviour
	{
		[SerializeField] private UIImage mMech;
		[SerializeField] private UIImage mBarrel;
		[SerializeField] private UIImage mGrip;
		[SerializeField] private UIImage mScope;

		private void Awake()
		{
			EventManager.Local.OnLocalPlayerAttachedPart += OnLocalPlayerAttachedPart;
		}

		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerAttachedPart -= OnLocalPlayerAttachedPart;
		}

		private void OnLocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript part)
		{
			switch (part.attachPoint)
			{
				case BaseWeaponScript.Attachment.Scope:
					mScope.sprite = part.durabilitySprite;
					mScope.color = Color.white;
					break;
				case BaseWeaponScript.Attachment.Barrel:
					mBarrel.sprite = part.durabilitySprite;
					mBarrel.color = Color.white;
					break;
				case BaseWeaponScript.Attachment.Mechanism:
					mMech.sprite = part.durabilitySprite;
					mMech.color = Color.white;
					break;
				case BaseWeaponScript.Attachment.Grip:
					mGrip.sprite = part.durabilitySprite;
					mMech.color = Color.white;
					break;
			}
		}
	}
}
