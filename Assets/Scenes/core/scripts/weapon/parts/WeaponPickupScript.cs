using System.Collections;
using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class WeaponPickupScript : MonoBehaviour, IInteractable
	{
		[SerializeField] private float mPickupScale = 2.0f;
		[SerializeField] private Collider mPickupCollider;
		
		private Rigidbody mPickupRigidbody;
		private WeaponPartScript mPart;

		private void Awake()
		{
			mPickupRigidbody = GetComponent<Rigidbody>();
			mPart = GetComponent<WeaponPartScript>();
		}

		private void Start()
		{
			StartCoroutine(Coroutines.InvokeAfterFrames(3, () =>
			{
				transform.localScale = Vector3.one * mPickupScale;

				GameObject psPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_pickupEffectPack");
				GameObject ps = Instantiate(psPrefab);

				ps.transform.SetParent(mPickupCollider.transform, false);
				ps.transform.position = mPickupCollider.bounds.center;
			}));
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
		}
		
		public void Interact()
		{
			Interact(null);
		}

		public void Interact(ICharacter source)
		{
			IWeaponBearer bearer = source as IWeaponBearer;

			if (bearer != null)
				ConfirmAttach(bearer.weapon);
			else
				ConfirmAttach();
		}

		public WeaponPickupScript OverrideDurability(int value)
		{
			mPart.durability = value;
			return this;
		}

		private void ConfirmAttach()
		{
			EventManager.Notify(() => EventManager.ConfirmPartAttach(mPart));
			Destroy(mPickupCollider.gameObject);
			Destroy(mPickupRigidbody);
			Destroy(this);
		}

		public void ConfirmAttach(IWeapon weapon)
		{
			Destroy(mPickupCollider.gameObject);
			Destroy(mPickupRigidbody);
			StartCoroutine(DirectAttach(weapon));
		}

		private IEnumerator DirectAttach(IWeapon weapon)
		{
			yield return null; //wait one tick
			weapon.AttachNewPart(mPart);
			Destroy(this);
		}
	}
}
