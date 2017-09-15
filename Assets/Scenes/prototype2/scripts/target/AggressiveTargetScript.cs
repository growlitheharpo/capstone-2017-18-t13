using System.Collections;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class AggressiveTargetScript : MonoBehaviour, IWeaponBearer
	{
		[SerializeField] private AIWeaponScript mWeapon;
		[SerializeField] private GameObject mDefaultScope;
		[SerializeField] private GameObject mDefaultBarrel;
		[SerializeField] private GameObject mDefaultMechanism;

		private BoundProperty<float> mHealthProp;

		// Use this for initialization
		private void Start()
		{
			mWeapon.bearer = this;
			Instantiate(mDefaultMechanism).GetComponent<WeaponPickupScript>().ConfirmAttach(mWeapon);
			Instantiate(mDefaultBarrel).GetComponent<WeaponPickupScript>().ConfirmAttach(mWeapon);
			Instantiate(mDefaultScope).GetComponent<WeaponPickupScript>().ConfirmAttach(mWeapon);

			StartCoroutine(FireLoop());
		}

		private IEnumerator FireLoop()
		{
			yield return null;
			mHealthProp = GetComponent<SampleTargetScript>().health;

			while (true)
			{
				yield return null;

				if (mHealthProp.value <= 0.0f)
					continue;

				mWeapon.FireWeapon();
			}
		}
		
		public Transform eye { get { return transform; } }

		public void ApplyRecoil(Vector3 direction, float amount)
		{
			// don't do anything
		}
	}
}
