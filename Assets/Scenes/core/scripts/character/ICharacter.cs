using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Base Character interface.
	/// </summary>
	public interface ICharacter
	{
		NetworkInstanceId netId { get; }
		GameObject gameObject { get; }
		Transform transform { get; }
		Transform eye { get; }

		bool isCurrentPlayer { get; }
	}
}
