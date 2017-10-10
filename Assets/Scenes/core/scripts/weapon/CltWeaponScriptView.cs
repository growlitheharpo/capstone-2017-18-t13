using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CltWeaponScriptView : NetworkBehaviour
{
	[ClientRpc]
	public void Reload()
	{
		// play animation
	}

	[ClientRpc]
	public void ShowShotAnimation()
	{
		
	}
}
