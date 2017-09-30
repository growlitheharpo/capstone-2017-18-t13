using System.Collections;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class WeaponPickupScript : NetworkBehaviour, IInteractable
	{
		[SerializeField] private Collider mPickupCollider;
		private Rigidbody mPickupRigidbody;
		private WeaponPartScript mPart;

		private void Awake()
		{
			mPickupRigidbody = GetComponent<Rigidbody>();
			mPart = GetComponent<WeaponPartScript>();
		}

		public void Interact(ICharacter source)
		{
			PlayerScript bearer = source as PlayerScript;

			if (bearer != null)
				bearer.CmdPickupNewPart(netId);
		}

		public WeaponPickupScript OverrideDurability(int value)
		{
			mPart.durability = value;
			return this;
		}
		
		public void ConfirmAttach(IWeapon weapon)
		{
			Destroy(mPickupCollider.gameObject);
			Destroy(mPickupRigidbody);

			StartCoroutine(Coroutines.WaitOneFrame(() =>
			{
				weapon.AttachNewPart(mPart);
				Destroy(this);
			}));
		}
	}
}
