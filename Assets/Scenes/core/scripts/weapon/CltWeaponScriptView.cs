using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CltWeaponScriptView : NetworkBehaviour
{
	[ClientRpc]
	public void RpcReload()
	{
		// play animation
	}

	[ClientRpc]
	public void RpcShowShotAnimation()
	{
		
	}
}
