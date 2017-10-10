using System;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

public class CltPlayer : NetworkBehaviour, IWeaponBearer, IDamageReceiver
{
	public bool isCurrentPlayer { get { return isLocalPlayer; } }

	public Transform eye { get { throw new NotImplementedException(); } }
	public IWeapon weapon { get { throw new NotImplementedException(); } }
	public WeaponDefaultsData defaultParts { get { throw new NotImplementedException(); } }

	private IPlayerHitIndicator mHitIndicator;

	public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
	{
		if (!isServer)
			throw new ArgumentException("Cannot apply damage from a local client!");

		throw new NotImplementedException();
		RpcReflectDamageLocally(point, normal, cause.source.gameObject.transform.position, amount);
	}

	public override void OnStartServer()
	{
		// register for server events
		mHitIndicator = new NullHitIndicator();
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

	// TODO: Is this the best way to handle this?
	[ClientRpc]
	private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount)
	{
		mHitIndicator.NotifyHit(this, origin, amount);
	}
}
