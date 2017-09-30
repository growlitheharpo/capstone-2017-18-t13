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

		private Transform mView, mRotator;

		private void Awake()
		{
			mPickupRigidbody = GetComponent<Rigidbody>();
			mPart = GetComponent<WeaponPartScript>();
			mView = transform.Find("View");

			mRotator = new GameObject("Rotator", typeof(RotatorUtilityScript)).transform;
			mRotator.SetParent(transform, false);
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

				mRotator.position = mPickupCollider.bounds.center;
				mView.SetParent(mRotator, true);
			}));
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
			mView.SetParent(transform);
			mView.localPosition = Vector3.zero;
			mView.localRotation = Quaternion.identity;
			mView.localScale = Vector3.one;

			Destroy(mRotator.gameObject);
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
