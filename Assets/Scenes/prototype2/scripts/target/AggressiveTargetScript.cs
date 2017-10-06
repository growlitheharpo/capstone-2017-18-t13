using System.Collections;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	// TODO: DELETE THIS CLASS
	public class AggressiveTargetScript : MonoBehaviour, IWeaponBearer
	{
		[SerializeField] private AIWeaponScript mWeapon;
		[SerializeField] private GameObject mDefaultScope;
		[SerializeField] private GameObject mDefaultBarrel;
		[SerializeField] private GameObject mDefaultMechanism;

		private BoundProperty<float> mHealthProp;
		public IWeapon weapon { get { return mWeapon; } }
		public bool isCurrentPlayer { get { return false; } }
		public WeaponDefaultsData defaultParts { get { throw new System.NotSupportedException("Aggressive target cannot break its weapon."); }}

		// Use this for initialization
		private void Start()
		{
			mWeapon.bearer = this;
			Instantiate(mDefaultMechanism)
				.GetComponent<WeaponPickupScript>()
				.OverrideDurability(WeaponPartScript.INFINITE_DURABILITY)
				.ConfirmAttach(mWeapon);

			Instantiate(mDefaultBarrel)
				.GetComponent<WeaponPickupScript>()
				.OverrideDurability(WeaponPartScript.INFINITE_DURABILITY)
				.ConfirmAttach(mWeapon);

			Instantiate(mDefaultScope)
				.GetComponent<WeaponPickupScript>()
				.OverrideDurability(WeaponPartScript.INFINITE_DURABILITY)
				.ConfirmAttach(mWeapon);

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

				mWeapon.FireWeaponHold();
			}
		}
		
		public Transform eye { get { return transform; } }

		public void ApplyRecoil(Vector3 direction, float amount)
		{
			// don't do anything
		}
	}
}
