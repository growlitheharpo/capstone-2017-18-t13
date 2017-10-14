using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Base Character interface.
/// </summary>
public interface ICharacter
{
	NetworkInstanceId netId { get; }
	GameObject gameObject { get; }
	Transform eye { get; }

	bool isCurrentPlayer { get; }
}
