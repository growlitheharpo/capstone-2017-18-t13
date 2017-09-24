using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class DemoWeaponScript : BaseWeaponScript
	{
		[SerializeField] private float mRotationRate;

		public WeaponData currentStats { get { return mCurrentData; } }

		protected override void PlayShotEffect(Vector3 a) { }
		protected override void PlayReloadEffect(float time) { }

		protected override void Awake()
		{
			base.Awake();

			mClipSize = new BoundProperty<int>(0, GameplayUIManager.CLIP_TOTAL);
			mAmountInClip = new BoundProperty<int>(0, GameplayUIManager.CLIP_CURRENT);
		}

		protected override void Update()
		{
			base.Update();
			transform.Rotate(Vector3.up, mRotationRate * Time.deltaTime);
		}
	}
}
