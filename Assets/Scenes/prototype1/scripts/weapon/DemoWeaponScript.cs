using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class DemoWeaponScript : BaseWeaponScript
	{
		[SerializeField] private float mRotationRate;

		public WeaponData currentStats { get { return mCurrentData; } }

		protected override void PlayShotEffect() { }
		protected override void PlayReloadEffect(float time) { }

		protected override void Update()
		{
			base.Update();
			transform.Rotate(Vector3.up, mRotationRate * Time.deltaTime);
		}
	}
}
