using UnityEngine.Networking;

namespace FiringSquad.Networking
{
	public interface INetworkable
	{
		void Serialize(NetworkWriter writer);
		void Deserialize(NetworkReader reader, out object target);
	}

	public interface INetworkable<out T> : INetworkable where T : new()
	{
		T Deserialize(NetworkReader reader);
	}
}
