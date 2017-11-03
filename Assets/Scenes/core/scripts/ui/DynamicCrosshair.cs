using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class DynamicCrosshair : MonoBehaviour
	{
		[SerializeField] private RectTransform mImageHolder;
		private CltPlayer mPlayerRef;

		private void Awake()
		{
			EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
		}

		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
		}

		private void OnLocalPlayerSpawned(CltPlayer obj)
		{
			mPlayerRef = obj;
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
		}

		private void Update()
		{
			if (mPlayerRef == null)
				return;

			IWeapon weapon = mPlayerRef.weapon;
			if (weapon == null)
				return;

			float scaleVal3 = Mathf.Tan(Mathf.Asin(weapon.GetCurrentDispersionFactor(true)));
			
			Vector3 cameraPos = mPlayerRef.eye.position;
			Vector3 rightPos = cameraPos + mPlayerRef.eye.forward + mPlayerRef.eye.right * scaleVal3;
			Vector3 upPos = cameraPos + mPlayerRef.eye.forward + mPlayerRef.eye.up * scaleVal3;

			Vector3 screenRight = Camera.main.WorldToScreenPoint(rightPos);
			Vector3 viewportRight = Camera.main.ScreenToViewportPoint(screenRight);
			Vector3 screenUp = Camera.main.WorldToScreenPoint(upPos);
			Vector3 viewportUp = Camera.main.ScreenToViewportPoint(screenUp);

			float width = Mathf.Abs(0.5f - viewportRight.x);
			float height = Mathf.Abs(0.5f - viewportUp.y);

			mImageHolder.anchorMax = new Vector2(0.5f + width, 0.5f + height);
			mImageHolder.anchorMin = new Vector2(0.5f - width, 0.5f - height);
		}
	}
}
