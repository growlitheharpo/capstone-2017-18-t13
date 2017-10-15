using System;
using FiringSquad.Gameplay;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponPickupScript : NetworkBehaviour, IInteractable, INetworkGrabbable
{
	[SerializeField] private GameObject mGunView;
	[SerializeField] private GameObject mPickupView;

	public CltPlayer currentHolder { get; private set; }
	public bool currentlyHeld { get { return currentHolder != null; } }

	private Rigidbody mRigidbody;

	private void Awake()
	{
		mRigidbody = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		InitializePickupView();
	}

	private void OnDestroy()
	{
		DestroyPickupView();

		transform.ResetLocalValues();
		mGunView.transform.ResetLocalValues();
		
		if (mRigidbody != null)
			Destroy(mRigidbody);
	}

	private void DestroyPickupView()
	{
		mGunView.SetActive(true);

		if (mPickupView != null)
			Destroy(mPickupView);
	}

	[ClientRpc]
	public void RpcInitializePickupView()
	{
		InitializePickupView();
	}

	public void InitializePickupView()
	{
		if (!mGunView.activeInHierarchy && mPickupView.activeInHierarchy)
			return;

		mGunView.SetActive(false);
		mPickupView.SetActive(true);

		GameObject psPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_pickupEffectPack");
		GameObject ps = Instantiate(psPrefab);

		ps.transform.SetParent(mPickupView.transform);
		ps.transform.ResetLocalValues();
	}

	[Server]
	public void Interact(ICharacter source)
	{
		IWeaponBearer wepBearer = source as IWeaponBearer;
		if (wepBearer == null)
			return;

		try
		{
			NetworkServer.Destroy(gameObject);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}

		Destroy(gameObject);

		wepBearer.weapon.AttachNewPart(GetComponent<WeaponPartScript>().partId);
	}

	public void PullTowards(CltPlayer player)
	{
		if (currentlyHeld)
			return;

		Vector3 direction = player.magnetArm.transform.position - transform.position;
		direction = direction.normalized * player.magnetArm.pullForce;

		mRigidbody.AddForce(direction, ForceMode.Force);
	}

	public void GrabNow(CltPlayer player)
	{
		currentHolder = player;
	}

	public void Throw()
	{
		Vector3 direction = currentHolder.magnetArm.transform.forward;

		transform.SetParent(null);
		currentHolder = null;
	}

	public void Release()
	{
		currentHolder = null;
	}
}
