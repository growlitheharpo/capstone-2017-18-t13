using System;
using FiringSquad.Gameplay;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponPickupScript : NetworkBehaviour, IInteractable
{
	[SerializeField] private GameObject mGunView;
	[SerializeField] private GameObject mPickupView;

	private Rigidbody mRigidbody;

	private void Awake()
	{
		mRigidbody = GetComponent<Rigidbody>();
	}

	private void OnDestroy()
	{
		DestroyPickupView();

		transform.ResetLocalValues();
		mGunView.transform.ResetLocalValues();
		
		if (mRigidbody != null)
			Destroy(mRigidbody);
	}

	public void InitializePickupView()
	{
		mGunView.SetActive(false);
		mPickupView.SetActive(true);

		GameObject psPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_pickupEffectPack");
		GameObject ps = Instantiate(psPrefab);

		ps.transform.SetParent(mPickupView.transform);
		ps.transform.ResetLocalValues();
	}

	public void DestroyPickupView()
	{
		mGunView.SetActive(true);

		if (mPickupView != null)
			Destroy(mPickupView);
	}

	[Server]
	public void Interact(ICharacter source)
	{
		IWeaponBearer wepBearer = source as IWeaponBearer;
		if (wepBearer == null)
			return;

		try
		{
			Network.Destroy(gameObject);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

		Destroy(gameObject);

		wepBearer.weapon.AttachNewPart(GetComponent<WeaponPartScript>().partId);
	}
}
