using System.Collections;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class WeaponPickupScript : NetworkBehaviour, IInteractable
	{
		[SerializeField] private float mPickupScale = 2.0f;
		[SerializeField] private Collider mPickupCollider;

		public BaseWeaponScript.Attachment attachPoint { get { return mPart.attachPoint; } }
		
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
		
		public void Interact(ICharacter source)
		{
			PlayerScript bearer = source as PlayerScript;

			if (bearer != null)
				bearer.CmdPickupNewPart(netId, this.name);
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
