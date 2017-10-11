using System;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

public class CltPlayer : NetworkBehaviour, IWeaponBearer, IDamageReceiver
{
	public bool isCurrentPlayer { get { return isLocalPlayer; } }

	public IWeapon weapon { get; private set; }
	public Transform eye { get { throw new NotImplementedException(); } }
	public WeaponPartCollection defaultParts { get { throw new NotImplementedException(); } }

	private IPlayerHitIndicator mHitIndicator;

	public override void OnStartServer()
	{
		// register for server events
		mHitIndicator = new NullHitIndicator();

		// create our weapon with client authority & bind
		BaseWeaponScript wep = new BaseWeaponScript(); // actually create it here
		BindWeaponToPlayer(wep);
	}

	public override void OnStartClient()
	{
		// register for local events that should effect all players (might not be any?)

		if (isLocalPlayer)
		{
			// instantiate the local player stuff
			// register for local-player only client events
			mHitIndicator = (IPlayerHitIndicator)FindObjectOfType<PlayerHitIndicator>() ?? new NullHitIndicator();
		}
		else
		{
			// register anything specifically for non-local clients
			
			// TODO: Make spawning hit particles done through here
			mHitIndicator = new NullHitIndicator();
		}
	}

	public void BindWeaponToPlayer(BaseWeaponScript wep)
	{
		// find attach spot in view and set parent
		weapon.transform.SetParent(transform);
		wep.bearer = this;
		weapon = weapon;
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
	public void WeaponFireHold()
	{
		weapon.FireWeaponHold();
	}

	[Command]
	public void WeaponFireUp()
	{
		weapon.FireWeaponUp();
	}

	[Command]
	public void WeaponReload()
	{
		weapon.Reload();
	}
}
