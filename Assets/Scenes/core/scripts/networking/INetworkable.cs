using UnityEngine.Networking;

namespace FiringSquad.Networking
{
	/// <summary>
	/// Base interface for an object that can be serialized and deserialized into a UNet stream.
	/// </summary>
	public interface INetworkable
	{
		/// <summary>
		/// Write this object to the provided stream.
		/// </summary>
		void Serialize(NetworkWriter writer);

		/// <summary>
		/// Read an object from the provided stream and write it to the target.
		/// </summary>
		void Deserialize(NetworkReader reader, out object target);
	}

	/// <summary>
	/// Base interface for a struct that can be serialized and deserialized into a UNet stream.
	/// </summary>
	/// <typeparam name="T">The struct's type.</typeparam>
	public interface INetworkable<out T> : INetworkable where T : struct
	{
		T Deserialize(NetworkReader reader);
	}
}
