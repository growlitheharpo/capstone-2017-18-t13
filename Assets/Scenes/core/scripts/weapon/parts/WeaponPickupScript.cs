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

	private WeaponPartWorldCanvas mCanvas;
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

		GameObject cvPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_partWorldCanvas");
		GameObject cv = Instantiate(cvPrefab, transform);
		mCanvas = cv.GetComponent<WeaponPartWorldCanvas>();
		mCanvas.LinkToObject(GetComponent<WeaponPartScript>());
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
			Debug.LogException(e);
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

		// TODO: Lerp this

		mPickupView.transform.localScale = Vector3.one * 0.45f;
		//mPickupView.SetActive(false);
		//mGunView.SetActive(true);

		mRigidbody.isKinematic = true;

		transform.SetParent(currentHolder.magnetArm.transform);
		transform.ResetLocalValues();
	}

	public void Throw()
	{
		if (currentHolder == null)
			return;

		Vector3 direction = currentHolder.eye.forward;

		transform.SetParent(null);
		mRigidbody.isKinematic = false;

		mPickupView.transform.localScale = Vector3.one;
		//mPickupView.SetActive(true);
		//mGunView.SetActive(false);

		mRigidbody.AddForce(direction * 30.0f, ForceMode.Impulse);

		currentHolder = null;
	}

	public void Release()
	{
		transform.SetParent(null);

		mRigidbody.isKinematic = false;

		mPickupView.transform.localScale = Vector3.one;
		//mPickupView.SetActive(true);
		//mGunView.SetActive(false);

		currentHolder = null;
	}
}
