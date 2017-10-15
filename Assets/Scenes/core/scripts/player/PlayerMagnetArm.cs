using UnityEngine;
using UnityEngine.Networking;

public class PlayerMagnetArm : NetworkBehaviour
{
	public CltPlayer bearer { get; set; }

	#region Serialization

	// Todo: Optimize these to only send changes
	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		writer.Write(bearer.netId);
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		// read our bearer
		NetworkInstanceId bearerId = reader.ReadNetworkId();
		if (bearer == null || bearer.netId != bearerId)
		{
			GameObject bearerObj = ClientScene.FindLocalObject(bearerId);
			if (bearerObj != null)
				bearerObj.GetComponent<CltPlayer>().BindMagnetArmToPlayer(this);
		}
	}

	#endregion
}
