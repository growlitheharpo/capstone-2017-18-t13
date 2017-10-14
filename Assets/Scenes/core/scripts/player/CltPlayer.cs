using System.Collections.Generic;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

public class CltPlayer : NetworkBehaviour, IWeaponBearer, IDamageReceiver
{
	[SerializeField] private PlayerAssetReferences mAssets;
	[SerializeField] private PlayerDefaultsData mInformation;

	[SerializeField] private Transform mCameraOffset;
	[SerializeField] private Transform mGun1Offset;
	[SerializeField] private Transform mGun2Offset;

	public bool isCurrentPlayer { get { return isLocalPlayer; } }

	public IWeapon weapon { get; private set; }
	public WeaponPartCollection defaultParts { get { return mInformation.defaultWeaponParts; } }
	public Transform eye { get { return mCameraOffset; } }

	private IPlayerHitIndicator mHitIndicator;

	[SyncVar(hook = "OnHealthUpdate")] private float mHealth;
	private BoundProperty<float> mLocalHealthVar;

	public override void OnStartServer()
	{
		// register for server events
		EventManager.Server.OnPlayerFiredWeapon += OnPlayerFiredWeapon;

		mHitIndicator = new NullHitIndicator();
		mHealth = mInformation.defaultHealth;

		// create our weapon & bind
		BaseWeaponScript wep = Instantiate(mAssets.baseWeaponPrefab).GetComponent<BaseWeaponScript>();
		BindWeaponToPlayer(wep);
		AddDefaultPartsToWeapon(wep);
		NetworkServer.SpawnWithClientAuthority(wep.gameObject, gameObject);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		Debug.Log("Client!");
		// register for local events that should effect all players (might not be any?)

		// register anything specifically for non-local clients
		// TODO: Make spawning hit particles done through here
		mHitIndicator = new NullHitIndicator();
		mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth);
	}

	public override void OnStartLocalPlayer()
	{
		// register for local events that should effect us

		CltPlayerLocal localScript = Instantiate(mAssets.localPlayerPrefab).GetComponent<CltPlayerLocal>();
		localScript.transform.SetParent(transform);
		localScript.playerRoot = this;

		mHitIndicator = (IPlayerHitIndicator)FindObjectOfType<PlayerHitIndicator>() ?? new NullHitIndicator();
		mLocalHealthVar = new BoundProperty<float>(mInformation.defaultHealth, GameplayUIManager.PLAYER_HEALTH);
	}

	public void BindWeaponToPlayer(BaseWeaponScript wep)
	{
		// find attach spot in view and set parent
		wep.transform.SetParent(mGun1Offset);
		wep.aimRoot = eye;
		wep.transform.ResetLocalValues();
		wep.positionOffset = eye.InverseTransformPoint(mGun1Offset.position);
		wep.transform.SetParent(transform);
		wep.bearer = this;
		weapon = wep;
	}

	private void AddDefaultPartsToWeapon(BaseWeaponScript wep)
	{
		foreach (WeaponPartScript part in defaultParts)
			wep.AttachNewPart(part.partId, true);
	}

	[Server]
	public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
	{
		if (ReferenceEquals(cause.source, this))
			amount *= 0.5f;

		mHealth -= amount;
		RpcReflectDamageLocally(point, normal, cause.source.gameObject.transform.position, amount);
	}

	[ClientRpc]
	private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount)
	{
		mHitIndicator.NotifyHit(this, origin, amount);
	}

	private void OnPlayerFiredWeapon(CltPlayer p, List<Ray> shots)
	{
		if (p != this)
			RpcReflectPlayerShotWeapon(p.netId);
	}

	[ClientRpc]
	private void RpcReflectPlayerShotWeapon(NetworkInstanceId playerId)
	{
		if (playerId == netId)
			return;

		CltPlayer p = ClientScene.FindLocalObject(playerId).GetComponent<CltPlayer>();
		p.weapon.PlayFireEffect();
	}

	[Command]
	public void CmdActivateInteract(Vector3 eyePosition, Vector3 eyeForward)
	{
		IInteractable interactable;
		RaycastHit hit;

		Ray ray = new Ray(eyePosition, eyeForward);
		if (!Physics.Raycast(ray, out hit, mInformation.interactDistance))
			return;

		interactable = hit.GetInteractableComponent();

		if (interactable != null)
			interactable.Interact(this);
	}

	[Command]
	public void CmdDebugEquipWeaponPart(string part)
	{
		weapon.AttachNewPart(part);
	}

	[Client]
	private void OnHealthUpdate(float value)
	{
		mHealth = value;
		if (mLocalHealthVar != null)
			mLocalHealthVar.value = value;
	}
}
