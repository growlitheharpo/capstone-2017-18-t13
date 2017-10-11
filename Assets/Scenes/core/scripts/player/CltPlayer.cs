using System;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

public class CltPlayer : NetworkBehaviour, IWeaponBearer, IDamageReceiver
{
	[SerializeField] private PlayerAssetReferences mAssets;
	[SerializeField] private PlayerDefaultsData mInformation;

	[SerializeField] private Transform mCameraOffset;

	public bool isCurrentPlayer { get { return isLocalPlayer; } }

	public IWeapon weapon { get; private set; }
	public WeaponPartCollection defaultParts { get { return mInformation.defaultWeaponParts; } }
	public Transform eye { get { return mCameraOffset; } }

	private IPlayerHitIndicator mHitIndicator;

	public override void OnStartServer()
	{
		// register for server events
		mHitIndicator = new NullHitIndicator();

		Debug.Log("Server!");

		// create our weapon with client authority & bind
		//BaseWeaponScript wep = new BaseWeaponScript(); // actually create it here
		//BindWeaponToPlayer(wep);
		BaseWeaponScript wep = Instantiate(mAssets.baseWeaponPrefab).GetComponent<BaseWeaponScript>();
		BindWeaponToPlayer(wep);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		Debug.Log("Client!");
		// register for local events that should effect all players (might not be any?)

		if (isLocalPlayer)
		{
			// instantiate the local player stuff
			// register for local-player only client events

			CltPlayerLocal localScript = Instantiate(mAssets.localPlayerPrefab).GetComponent<CltPlayerLocal>();
			localScript.playerRoot = this;

			mHitIndicator = (IPlayerHitIndicator)FindObjectOfType<PlayerHitIndicator>() ?? new NullHitIndicator();
		}
		else
		{
			// register anything specifically for non-local clients

			Debug.Log("Not the local player!");
			// TODO: Make spawning hit particles done through here
			mHitIndicator = new NullHitIndicator();
		}
	}

	public override void OnStartLocalPlayer()
	{
		CltPlayerLocal localScript = Instantiate(mAssets.localPlayerPrefab).GetComponent<CltPlayerLocal>();
		localScript.transform.SetParent(transform);
		localScript.playerRoot = this;

		mHitIndicator = (IPlayerHitIndicator)FindObjectOfType<PlayerHitIndicator>() ?? new NullHitIndicator();
	}

	public void BindWeaponToPlayer(BaseWeaponScript wep)
	{
		// find attach spot in view and set parent
		wep.transform.SetParent(transform);
		wep.bearer = this;
		weapon = wep;
	}

	[Server]
	public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
	{
		throw new NotImplementedException();
		RpcReflectDamageLocally(point, normal, cause.source.gameObject.transform.position, amount);
	}

	// TODO: Is this the best way to handle this?
	[ClientRpc]
	private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount)
	{
		mHitIndicator.NotifyHit(this, origin, amount);
	}

	[Command]
	public void CmdWeaponFireHold()
	{
		weapon.FireWeaponHold();
	}

	[Command]
	public void CmdWeaponFireUp()
	{
		weapon.FireWeaponUp();
	}

	[Command]
	public void CmdWeaponReload()
	{
		weapon.Reload();
	}

	[Command]
	public void CmdActivateInteract()
	{

	}
}
