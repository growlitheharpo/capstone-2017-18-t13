using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Base Networked Character interface.
	/// All characters must be networked to validly implement this interface.
	/// </summary>
	public interface ICharacter
	{
		/// <summary>
		/// The NetworkId of this character.
		/// </summary>
		NetworkInstanceId netId { get; }
		
		/// <summary>
		/// The root GameObject of this character.
		/// </summary>
		GameObject gameObject { get; }

		/// <summary>
		/// The root transform of this character.
		/// TODO: Make a "read-only" transform interface.
		/// </summary>
		Transform transform { get; }

		/// <summary>
		/// The eye transform of this character.
		/// Where they "see" from.
		/// </summary>
		Transform eye { get; }

		/// <summary>
		/// True if this character is the local player. False otherwise.
		/// </summary>
		bool isCurrentPlayer { get; }
	}
}
