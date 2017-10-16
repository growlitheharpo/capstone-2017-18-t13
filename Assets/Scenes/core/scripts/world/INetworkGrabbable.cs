using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public interface INetworkGrabbable
	{
		NetworkInstanceId netId { get; }
		CltPlayer currentHolder { get; }
		bool currentlyHeld { get; }

		Transform transform { get; }
		GameObject gameObject { get; }

		void PullTowards(CltPlayer player);
		void GrabNow(CltPlayer player);
		void Throw();
		void Release();
	}
}
