using UnityEngine;

namespace Prototype2
{
	public class WeaponPickupScript : MonoBehaviour, IInteractable
	{
		[SerializeField] private Collider mPickupCollider;
		private Rigidbody mPickupRigidbody;
		private WeaponPartScript mPart;

		private void Awake()
		{
			mPickupRigidbody = GetComponent<Rigidbody>();
			mPart = GetComponent<WeaponPartScript>();
		}

		public void Interact()
		{
			// TODO: Open a menu and give player the choice over whether or not to attach
			ConfirmAttach();
		}

		public void ConfirmAttach()
		{
			EventManager.Notify(() => EventManager.ConfirmPartAttach(mPart));
			Destroy(mPickupCollider.gameObject);
			Destroy(mPickupRigidbody);
			Destroy(this);
		}
	}
}
